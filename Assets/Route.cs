using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class Route : MonoBehaviour
{
    public static bool guided = false;
    [SerializeField] private Transform[] waypoints;
    private int index = 0;
    public int Index
    {
        get => index; set
        {
            index = value;
            if (index >= waypoints.Length)
                SceneManager.LoadScene("EndScene");
        }
    }

    void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        if (guided)
        {
            //bike.position = waypoints[0].position;
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = waypoints.Length;
            List<Vector3> line = new();
            foreach (Transform t in waypoints)
            {
                line.Add(t.position + Vector3.up);
            }

            Debug.Log("Line: " + line.Count);
            lineRenderer.SetPositions(line.ToArray());


        }
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CheckWaypoint(Vector3 position)
    {
        if (!guided)
            return true;

        if (position == waypoints[Index].position)
        {
            Index++;
            return true;
        }

        int waypointIndex = Array.FindIndex(waypoints, (x) => x.position == position);

        if (waypointIndex == -1)
        {
            return false;
        }
        else if (waypointIndex > Index)
        {
            Index = waypointIndex + 1;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints.Length <= 0)
            return;

        Transform firstNode = waypoints[0];
        foreach (Transform w in waypoints)
        {
            Gizmos.DrawLine(firstNode.position + Vector3.up, w.position + Vector3.up);
            firstNode = w;
        }
    }
}
