using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Character : MonoBehaviour
{
    private UIController uiController;
    private SplineFollower follower;
    private 
    void Start ()
    {
        uiController = UIController.Instance;
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
        if (other.tag == "TargetPoint")
        {
            // If character has moved to next level
            var targetPoint = other.GetComponent<TargetPoint>();
            if (targetPoint.Level == LevelsController.Instance.CurrentLevel)
            {
                Stop();
                ShowPanelPlayLevel(1f);
            }
        }
    }

    private void ShowPanelPlayLevel(float _delay)
    {
        StartCoroutine(ShowPanelPlayLevelCR(_delay));
    }

    private IEnumerator ShowPanelPlayLevelCR(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        uiController.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
    }
}
