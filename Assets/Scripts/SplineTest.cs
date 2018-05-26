using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class SplineTest : MonoBehaviour
{
    [SerializeField]
    private SplineComputer spline;
    [SerializeField]
    private SplineFollower follower;

    private PathGenerator path;

    void Start()
    {
        path = spline.GetComponent<PathGenerator>();

        //follower.computer = computer; //Set the computer of the follower component
        //follower.followSpeed = 5f; //Set the speed of the follower
        //follower.wrapMode = SplineFollower.Wrap.PingPong; //Set the wrap mode of the         
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

    void OnBeginningReached()
    {
        Debug.Log("Beginning reached");
        //follower.motion.offset = Vector2.up;

    }

    void OnEndReached()
    {
        Debug.Log("End reached");
        //follower.motion.offset = Vector2.down;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
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
    }
}
