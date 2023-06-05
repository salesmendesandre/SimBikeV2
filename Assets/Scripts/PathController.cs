using GleyTrafficSystem;
using GleyUrbanAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class PathController : MonoBehaviour
{
    [SerializeField] private CurrentSceneData currentSceneData;
    [SerializeField] private Transform alternativeParent;
    [SerializeField] private GameObject alternativePath;
    [SerializeField] private LineRenderer currentPath;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private Rigidbody rb;

    private Waypoint[] mapWaypoints;
    private Waypoint prevWaypoint;
    private Waypoint currentWaypoint;
    private Waypoint nextWaypoint;

    private Timer velocityTimer = new(3000);
    private Vector3 direction;

    public UnityEvent OnOutOfRoad = new();
    public UnityEvent OnLaneChange = new();
    public UnityEvent OnInverseLaneChange = new();
    public UnityEvent OnRightPath = new();
    public UnityEvent OnSemaforeFail = new();
    public UnityEvent OnNoMovement = new();

    private bool velocityAlert = false;
    void Start()
    {
        mapWaypoints = currentSceneData.allWaypoints;
        currentPath = GetComponent<LineRenderer>();
        velocityTimer.AutoReset = false;
        velocityTimer.Elapsed += VelocityTimer_Elapsed;
    }


    // Update is called once per frame
    void Update()
    {
        if (currentWaypoint == null)
        {
            SearchNewWaypoint();
        }

        GetNextWaypoint();
        ShouldStop();
        DrawPath();

        if (velocityAlert)
        {
            OnNoMovement.Invoke();
            velocityAlert = false;
        }
    }

    private void ShouldStop()
    {
        if (currentWaypoint.stop && rb.velocity.sqrMagnitude >= 0.1f)
            OnSemaforeFail.Invoke();

        if (nextWaypoint.stop && Vector3.Distance(centerOfMass.position, nextWaypoint.position) < 5 && rb.velocity.magnitude < 0.1)
        {
            Debug.Log("Puedes pararte");
            if (velocityTimer.Enabled)
                velocityTimer.Stop();
        }
        else if (Physics.Raycast(centerOfMass.position, centerOfMass.forward, 10, LayerMask.GetMask("Traffic")))
        {
            Debug.Log("Puedes pararte");
            if (velocityTimer.Enabled)
                velocityTimer.Stop();
        }
        else if (rb.velocity.magnitude < 0.1)
        {
            Debug.Log("No puedes pararte");
            if (!velocityTimer.Enabled)
                velocityTimer.Start();
        }
    }


    private void VelocityTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        velocityAlert = true;
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

            OnRightPath.Invoke();
            return;
        }
        float pathDistance = DistanceToPath();

        if (closest == currentWaypoint)
        {
            OnRightPath.Invoke();
            return;
        }
        else if (pathDistance <= 2)
        {
            OnRightPath.Invoke();
            return; //Seguimos en camino
        }
        else
        {

            currentWaypoint = closest;
            prevWaypoint = mapWaypoints[currentWaypoint.prev[0]];
            nextWaypoint = mapWaypoints[currentWaypoint.neighbors[0]];
            
            direction = nextWaypoint.position - prevWaypoint.position;
           
            if (pathDistance > 7)
            {
                OnOutOfRoad.Invoke();

            }
            if (Vector3.Angle(currentDir, direction) < 5 && pathDistance < 7)
            {
                OnLaneChange.Invoke();

            }
            else if (Vector3.Angle(currentDir, direction) > 75 && pathDistance < 7)
            {
                OnInverseLaneChange.Invoke();

            }
            else
            {
                OnRightPath.Invoke();
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
        Vector3 postNextPos = mapWaypoints[nextWaypoint.neighbors[0]]?.position ?? centerOfMass.position;
        
        currentPath.positionCount = 4;
        currentPath.SetPositions(new Vector3[] { prevPos + Vector3.up / 2, currentPos + Vector3.up / 2, nextPos + Vector3.up / 2 , postNextPos  + Vector3.up / 2});


        foreach (Transform t in alternativeParent)
        {
            Destroy(t.gameObject);
        }

        if (currentWaypoint != null && prevWaypoint != null && nextWaypoint != null)
        {
            for (int i = 0; i < currentWaypoint.neighbors.Count; i++)
            {
                if (mapWaypoints[currentWaypoint.neighbors[i]] == nextWaypoint)
                    continue;
                List<Vector3> path = new();
                GameObject altPath = Instantiate(alternativePath, alternativeParent);
                path.Add(prevPos + Vector3.up / 2);
                path.Add(currentPos + Vector3.up / 2);
                path.Add(mapWaypoints[currentWaypoint.neighbors[i]].position + Vector3.up / 2);
                path.Add(mapWaypoints[mapWaypoints[currentWaypoint.neighbors[i]].neighbors[0]].position + Vector3.up / 2);
                altPath.GetComponent<LineRenderer>().positionCount = path.Count;
                altPath.GetComponent<LineRenderer>().SetPositions(path.ToArray());
            }
        }
    }
}
