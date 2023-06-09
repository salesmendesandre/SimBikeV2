using GleyTrafficSystem;
using GleyUrbanAssets;
using System;
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
    [SerializeField] private Route route;

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
    public UnityEvent OnOutOfRoute = new();

    private bool velocityAlert = false;

    public Waypoint CurrentWaypoint
    {
        get => currentWaypoint;
        set
        {
            currentWaypoint = value;
            if (!route.CheckWaypoint(value.position))
            {
                OnOutOfRoute.Invoke();
            }

        }
    }

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
        if (CurrentWaypoint == null)
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
        if (CurrentWaypoint.stop && rb.velocity.sqrMagnitude >= 0.1f)
            OnSemaforeFail.Invoke();

        if (nextWaypoint.stop && Vector3.Distance(centerOfMass.position, nextWaypoint.position) < 5 && rb.velocity.magnitude < 0.1)
        {
           
            if (velocityTimer.Enabled)
                velocityTimer.Stop();
        }
        else if (Physics.Raycast(centerOfMass.position, centerOfMass.forward, 10, LayerMask.GetMask("Traffic")))
        {
            
            if (velocityTimer.Enabled)
                velocityTimer.Stop();
        }
        else if (rb.velocity.magnitude < 0.1)
        {
           
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
    private Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector = lineEnd - lineStart;
        float magnitude = vector.magnitude;
        Vector3 vector2 = vector;
        if (magnitude > 1E-06f)
        {
            vector2 /= magnitude;
        }

        float value = Vector3.Dot(vector2, rhs);
        value = Mathf.Clamp(value, 0f, magnitude);
        return lineStart + vector2 * value;
    }
    private void DecideNextWaypoint()
    {
        float distance = float.PositiveInfinity;
        Waypoint tempWaypoint = nextWaypoint;
        foreach (int waypointIndex in CurrentWaypoint.neighbors)
        {
            //float nextAngle = Vector3.Angle(transform.forward, mapWaypoints[waypointIndex].position - currentWaypoint.position);
            //float nextDistance = HandleUtility.DistancePointLine(centerOfMass.position, CurrentWaypoint.position, mapWaypoints[waypointIndex].position);
            float nextDistance = Vector3.Magnitude(ProjectPointLine(centerOfMass.position, CurrentWaypoint.position, mapWaypoints[waypointIndex].position) - centerOfMass.position);
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

        if (CurrentWaypoint.neighbors.Contains(closestIndex))
        {
            if (Vector3.Distance(centerOfMass.position, nextWaypoint.position) > 1)
                return;

            prevWaypoint = CurrentWaypoint;
            CurrentWaypoint = nextWaypoint;
            nextWaypoint = mapWaypoints[CurrentWaypoint.neighbors[0]];

            direction = nextWaypoint.position - prevWaypoint.position;

            OnRightPath.Invoke();
            return;
        }
        float pathDistance = DistanceToPath();

        if (closest == CurrentWaypoint)
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

            CurrentWaypoint = closest;
            prevWaypoint = mapWaypoints[CurrentWaypoint.prev[0]];
            nextWaypoint = mapWaypoints[CurrentWaypoint.neighbors[0]];

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
        CurrentWaypoint = GetClosestWaypoint();
        prevWaypoint = mapWaypoints[CurrentWaypoint.prev[0]];
        nextWaypoint = mapWaypoints[CurrentWaypoint.neighbors[0]];

        direction = nextWaypoint.position - prevWaypoint.position;
    }

    private float DistanceToPath()
    {

        Vector3 vehiclePosition = centerOfMass.position;
        vehiclePosition.y = 0;
        return Vector3.Magnitude(ProjectPointLine(vehiclePosition, prevWaypoint.position, nextWaypoint.position) - vehiclePosition);
    }

    private void DrawPath()
    {
        if (!currentPath.enabled)
            return;

        Vector3 prevPos = prevWaypoint?.position ?? centerOfMass.position;
        Vector3 currentPos = CurrentWaypoint?.position ?? centerOfMass.position;
        Vector3 nextPos = nextWaypoint?.position ?? centerOfMass.position;
        Vector3 postNextPos = mapWaypoints[nextWaypoint.neighbors[0]]?.position ?? centerOfMass.position;

        currentPath.positionCount = 4;
        currentPath.SetPositions(new Vector3[] { prevPos + Vector3.up / 2, currentPos + Vector3.up / 2, nextPos + Vector3.up / 2, postNextPos + Vector3.up / 2 });


        foreach (Transform t in alternativeParent)
        {
            Destroy(t.gameObject);
        }

        if (CurrentWaypoint != null && prevWaypoint != null && nextWaypoint != null)
        {
            for (int i = 0; i < CurrentWaypoint.neighbors.Count; i++)
            {
                if (mapWaypoints[CurrentWaypoint.neighbors[i]] == nextWaypoint)
                    continue;
                List<Vector3> path = new();
                GameObject altPath = Instantiate(alternativePath, alternativeParent);
                path.Add(prevPos + Vector3.up / 2);
                path.Add(currentPos + Vector3.up / 2);
                path.Add(mapWaypoints[CurrentWaypoint.neighbors[i]].position + Vector3.up / 2);
                path.Add(mapWaypoints[mapWaypoints[CurrentWaypoint.neighbors[i]].neighbors[0]].position + Vector3.up / 2);
                altPath.GetComponent<LineRenderer>().positionCount = path.Count;
                altPath.GetComponent<LineRenderer>().SetPositions(path.ToArray());
            }
        }

    }

    public void ToggleLine()
    {
        currentPath.enabled = !currentPath.enabled;
        foreach (Transform t in alternativeParent)
        {
            Destroy(t.gameObject);
        }
    }
}
