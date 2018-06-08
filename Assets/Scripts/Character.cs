using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Character : MonoBehaviour
{
    // Hardcoded values for SplineFollower positions based on platform placement. Used to move character to any specific level/platform.
    [SerializeField]
    private float[] PlatformPositions;

    private UIController uiController;
    private MapsController mapsController;
    private LevelsController levelsController;
    private SplineFollower follower;
    private Collider sphereCollider;

    void Start ()
    {
        uiController = UIController.Instance;
        mapsController = MapsController.Instance;
        levelsController = LevelsController.Instance;
        follower = GetComponent<SplineFollower>();
        sphereCollider = GetComponent<Collider>();

        sphereCollider.enabled = false;
    }
	
    public void SetPosition(int level)
    {
        if (level <= PlatformPositions.Length)
        {
            float posPercentage = PlatformPositions[level-1];
            follower.SetPercent(posPercentage);
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
            if (targetPoint.Level == levelsController.CurrentLevel)
            {
                Stop();
                ShowPanelPlayLevel(1f);

                // Update targetpoint color
                mapsController.HighlightTargetPoint(targetPoint.Level);
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
