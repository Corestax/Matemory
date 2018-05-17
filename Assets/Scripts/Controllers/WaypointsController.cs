using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsController : MonoBehaviour
{
    [SerializeField]
    private Transform[] Waypoints;


	void Start ()
    {

	}
	
	void Update ()
    {
		
	}

    void OnDrawGizmos()
    {
        if(Waypoints != null && Waypoints.Length > 0)
        {
            Gizmos.color = Color.green;

            foreach (Transform t in Waypoints)
                Gizmos.DrawSphere(t.position, 0.01f);

            Gizmos.color = Color.red;

            for (int i = 0; i < Waypoints.Length - 1; i++)
                Gizmos.DrawLine(Waypoints[i].position, Waypoints[i + 1].position);
        }
    }
}
