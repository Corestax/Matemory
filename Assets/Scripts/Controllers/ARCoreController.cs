using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using GoogleARCore.HelloAR;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARCoreController : MonoBehaviour
{
    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
    /// </summary>
    public Camera FirstPersonCamera;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject TrackedPlanePrefab;

    /// <summary>
    /// A model to place when a raycast from a user touch hits a plane.
    /// </summary>
    public GameObject RoomPrefab;

    /// <summary>
    /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
    /// </summary>
    public GameObject Text_DetectingPlane;

    /// <summary>
    /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    private Anchor anchor;

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        _QuitOnConnectionErrors();
        
        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;

            if (!m_IsQuitting && Session.Status.IsValid())
                Text_DetectingPlane.SetActive(true);

            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        if (!anchor)
        {
            Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World coordinates.
                GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
            }
        }

        // Show/hide text "Detecting plane..."
        Session.GetTrackables<TrackedPlane>(m_AllPlanes);
        bool showText = true;
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                showText = false;
                break;
            }
        }
        Text_DetectingPlane.SetActive(showText);

        // DO NOT ALLOW REPOSITIONING
        if (anchor)
            return;

        // Show plane visualizer
        //if(showText)
        //    TogglePlaneVisualizer(true);

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            return;

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;
        
        // Reposition room
        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            // Check if the mouse was clicked over a UI element
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                print("Repositioning room...");
                //var roomPrefab = Instantiate(RoomPrefab, hit.Pose.position, hit.Pose.rotation);

                // Set room
                RoomController.Instance.Room = RoomPrefab;
                RoomPrefab.transform.position = hit.Pose.position;
                RoomPrefab.transform.rotation = hit.Pose.rotation;
                RoomPrefab.SetActive(true);

                GameController.Instance.Spawn(GameController.ModelTypes.GIRAFFE, true);

                // Position room 5 units in front of the camera
                //var pos = FirstPersonCamera.transform.forward * 10f;
                //pos.y = -5f;
                //RoomController.Instance.ShowRoom(hit.Pose.position);

                // Look at camera but still be flush with the plane.
                if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
                {
                    // Get the camera position and match the y-component with the hit position.
                    Vector3 pos = FirstPersonCamera.transform.position;
                    pos.y = hit.Pose.position.y;

                    RoomPrefab.transform.LookAt(pos, RoomPrefab.transform.up);
                    //RoomPrefab.transform.rotation = Quaternion.Euler(0.0f, RoomPrefab.transform.rotation.eulerAngles.y, RoomPrefab.transform.rotation.z);
                }

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical world evolves.
                if (!anchor)
                {
                    anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    RoomPrefab.transform.parent = anchor.transform;
                }

                // Hide plane visualizer
                TogglePlaneVisualizer(false);
            }
        }
    }

    public void TogglePlaneVisualizer(bool flag)
    {
        foreach (TrackedPlaneVisualizer tpv in Resources.FindObjectsOfTypeAll(typeof(TrackedPlaneVisualizer)) as TrackedPlaneVisualizer[])
        {
            tpv.GetComponent<Renderer>().enabled = flag;
            tpv.enabled = flag;
        }
    }
    
    /// <summary>
    /// Quit the application if there was a connection error for the ARCore session.
    /// </summary>
    private void _QuitOnConnectionErrors()
    {
        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
