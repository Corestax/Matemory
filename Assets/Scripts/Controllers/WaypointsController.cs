using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class WaypointsController : MonoBehaviour
{
    [SerializeField]
    private Transform[] Waypoints;

    [SerializeField]
    private SplineComputer spline;
    [SerializeField]
    private SplineFollower follower;

    private PathGenerator path;

    void Start ()
    {
        path = spline.GetComponent<PathGenerator>();
    }
	
    public void MoveTo(int level)
    {
        //follower.start
    }

    //private void OnEnable()
    //{
    //    follower.onBeginningReached += OnBeginningReached;
    //    follower.onEndReached += OnEndReached;
    //}

    //private void OnDisable()
    //{
    //    follower.onBeginningReached -= OnBeginningReached;
    //    follower.onEndReached -= OnEndReached;
    //}

    //private void OnBeginningReached()
    //{
    //    Debug.Log("Beginning reached");
    //    //follower.motion.offset = Vector2.up;
    //}

    //private void OnEndReached()
    //{
    //    Debug.Log("End reached");
    //    //follower.motion.offset = Vector2.down;
    //}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            path.SetClipRange(0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            path.SetClipRange(0, 0.5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            path.SetClipRange(0, 1);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            AddTrigger();
        }
    }

    private void AddTrigger()
    {
        follower.AddTrigger(Trigger.Type.Forward, CallTrigger, 0.5f, Trigger.Type.Double);
    }

    private void CallTrigger()
    {
        print("Calling trigger");
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
