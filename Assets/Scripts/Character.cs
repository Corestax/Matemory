﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Character : MonoBehaviour
{
    [SerializeField]
    private float[] PlatformPositions;

    private UIController uiController;
    private SplineFollower follower;
    private Collider sphereCollider;

    void Start ()
    {
        uiController = UIController.Instance;
        follower = GetComponent<SplineFollower>();
        sphereCollider = GetComponent<Collider>();

        sphereCollider.enabled = false;
    }
	
    public void SetPosition(int level)
    {
        if (level <= PlatformPositions.Length)
        {
            float position = PlatformPositions[level-1];
            print(level +": " + position);
            follower.SetDistance(position);
        }
    }

    public void Play(bool moveForward)
    {
        if (moveForward)        
            follower.direction = Spline.Direction.Forward;
        else
            follower.direction = Spline.Direction.Backward;

        follower.autoFollow = true;
        sphereCollider.enabled = true;
    }

    public void Stop()
    {
        follower.autoFollow = false;
        sphereCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "TargetPoint")
        {
            // If character has moved to next level
            var targetPoint = other.GetComponent<TargetPoint>();
            print(targetPoint.Level + " == " + LevelsController.Instance.CurrentLevel);
            if (targetPoint.Level == LevelsController.Instance.CurrentLevel)
            {
                Stop();
                ShowPanelPlayLevel(1f);
            }
        }
    }

    public void ShowPanelPlayLevel(float _delay)
    {
        StartCoroutine(ShowPanelPlayLevelCR(_delay));
    }

    private IEnumerator ShowPanelPlayLevelCR(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        uiController.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
    }
}
