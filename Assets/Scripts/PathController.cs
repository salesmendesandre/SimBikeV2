using GleyTrafficSystem;
using GleyUrbanAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PathController : MonoBehaviour
{
    [SerializeField] private CurrentSceneData currentSceneData;
    [SerializeField] private Transform alternativeParent;
    [SerializeField] private GameObject alternativePath;
    [SerializeField] private LineRenderer currentPath;
    [SerializeField] private Transform centerOfMass;

    private Waypoint[] mapWaypoints;
    private Waypoint prevWaypoint;
    private Waypoint currentWaypoint;
    private Waypoint nextWaypoint;
    private Waypoint bestWaypoint;

    private Vector3 direction;
    void Start()
    {
        mapWaypoints = currentSceneData.allWaypoints;
        currentPath = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentWaypoint == null)
        {
            SearchNewWaypoint();
        }

        GetNextWaypoint();
        DrawPath();
    }

    private Waypoint GetClosestWaypointDistance()
    {
        return Array.Find(mapWaypoints, (waypoint) => Vector3.Distance(waypoint.position, centerOfMass.position) < 1);
    }
    private Waypoint GetClosestWaypoint()
    {
        return mapWaypoints.Aggregate((p1, p2) => Vector3.Distance(p1.position, centerOfMass.position) < Vector3.Distance(p2.position, centerOfMass.position) ? p1 : p2);
    }
    private void DecideNextWaypoint()
    {
        float distance = float.PositiveInfinity;
        Waypoint tempWaypoint = nextWaypoint;
        foreach (int waypointIndex in currentWaypoint.neighbors)
        {
            //float nextAngle = Vector3.Angle(transform.forward, mapWaypoints[waypointIndex].position - currentWaypoint.position);
            float nextDistance = HandleUtility.DistancePointLine(centerOfMass.position, currentWaypoint.position, mapWaypoints[waypointIndex].position);

            if (nextDistance < distance)
            {

                distance = nextDistance;
                tempWaypoint = mapWaypoints[waypointIndex];

            }
        }
        if (nextWaypoint != tempWaypoint)
        {
            nextWaypoint = tempWaypoint;
            direction = nextWaypoint.position - prevWaypoint.position;
        }
    }
    private void GetNextWaypoint()
    {
        Waypoint closest = GetClosestWaypoint();
        int closestIndex = Array.IndexOf(mapWaypoints, closest);
        Vector3 currentDir = direction;

        DecideNextWaypoint();

        if (currentWaypoint.neighbors.Contains(closestIndex))
        {
            if (Vector3.Distance(centerOfMass.position, nextWaypoint.position) > 1)
                return;

            prevWaypoint = currentWaypoint;
            currentWaypoint = nextWaypoint;
            nextWaypoint = mapWaypoints[currentWaypoint.neighbors[0]];

            direction = nextWaypoint.position - prevWaypoint.position;

            return;
        }
        float pathDistance = DistanceToPath();

        if (closest == currentWaypoint)
        {
            return;
        }
        else if (pathDistance <= 2)
            return; //Seguimos en camino
        else
        {

            Debug.Log("Third waypoint!");
            currentWaypoint = closest;
            prevWaypoint = mapWaypoints[currentWaypoint.prev[0]];
            nextWaypoint = mapWaypoints[currentWaypoint.neighbors[0]];

            direction = nextWaypoint.position - prevWaypoint.position;

            if (pathDistance > 5)
                Debug.Log("Out of road");

            if (Vector3.Angle(currentDir, direction) < 2 && pathDistance < 5)
            {
                Debug.Log("Cambio de carril misma dirección");
            }
            else if (Vector3.Angle(currentDir, direction) > 175 && pathDistance < 5)
            {
                Debug.Log("Cambio de carril dirección cotraria");
            }
        }
    }

    private void SearchNewWaypoint()
    {
        currentWaypoint = GetClosestWaypoint();
        prevWaypoint = mapWaypoints[currentWaypoint.prev[0]];
        nextWaypoint = mapWaypoints[currentWaypoint.neighbors[0]];

        direction = nextWaypoint.position - prevWaypoint.position;
    }

    private float DistanceToPath()
    {

        Vector3 vehiclePosition = centerOfMass.position;
        vehiclePosition.y = 0;

        return HandleUtility.DistancePointLine(vehiclePosition, prevWaypoint.position, nextWaypoint.position);
    }

    private void DrawPath()
    {
        Vector3 prevPos = prevWaypoint?.position ?? centerOfMass.position;
        Vector3 currentPos = currentWaypoint?.position ?? centerOfMass.position;
        Vector3 nextPos = nextWaypoint?.position ?? centerOfMass.position;

        currentPath.positionCount = 3;
        currentPath.SetPositions(new Vector3[] { prevPos + Vector3.up / 2, currentPos + Vector3.up / 2, nextPos + Vector3.up / 2 });

        Debug.Log(currentWaypoint.neighbors.Count);
        foreach (Transform t in alternativeParent)
        {
            Destroy(t.gameObject);
        }

        if (currentWaypoint != null && prevWaypoint != null && nextWaypoint != null)
        {
            for (int i = 1; i < currentWaypoint.neighbors.Count; i++)
            {
                List<Vector3> path = new List<Vector3>();
                GameObject altPath = Instantiate(alternativePath, alternativeParent);
                path.Add(prevPos + Vector3.up / 2);
                path.Add(currentPos + Vector3.up / 2);
                path.Add(mapWaypoints[currentWaypoint.neighbors[i]].position + Vector3.up / 2);
                altPath.GetComponent<LineRenderer>().positionCount = path.Count;
                altPath.GetComponent<LineRenderer>().SetPositions(path.ToArray());
            }
        }
    }
}
