using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Character : MonoBehaviour
{
    private SplineFollower follower;

    void Start ()
    {
        follower = GetComponent<SplineFollower>();
	}
	
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.D))
            follower.clipFrom = 0.2f;
    }    

    public void SetPosition(float distance)
    {
        follower.SetDistance(distance);
    }

    public void Play()
    {
        follower.autoFollow = true;
    }

    public void Stop()
    {
        follower.autoFollow = false;        
    }

    private void OnTriggerEnter(Collider other)
    {
        Stop();
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
