using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.HelloAR;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARCoreController : MonoBehaviour
{
    [SerializeField]
    private Platform platform;

    public Camera FirstPersonCamera;
    public GameObject TrackedPlanePrefab;
    public GameObject RoomPrefab;

    private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();
    private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

    private bool m_IsQuitting = false;
    private Anchor anchor;
    private bool isTrackingLost;
    private Vector3 roomStartPos;
    private Touch touch;
    private TrackableHit hit;

    private const float MOVE_SPEED = 4f;

    public static event Action OnTrackingActive;
    public static event Action OnTrackingLost;

    void Start()
    {
        // Set room
        RoomController.Instance.Room = RoomPrefab;
        RoomPrefab.transform.SetParent(FirstPersonCamera.transform);
        RoomPrefab.SetActive(true);
        roomStartPos = RoomPrefab.transform.position;
    }

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

            //if (!m_IsQuitting && Session.Status.IsValid())
            //    Text_DetectingPlane.SetActive(true);

            isTrackingLost = true;
            if (OnTrackingLost != null)
                OnTrackingLost();
            return;
        }

        if (isTrackingLost)
        {
            isTrackingLost = false;
            if (OnTrackingActive != null)
                OnTrackingActive();
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

        // Get trackable planes
        //Session.GetTrackables<TrackedPlane>(m_AllPlanes);
        //bool showText = true;
        //for (int i = 0; i < m_AllPlanes.Count; i++)
        //{
        //    if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
        //    {
        //        showText = false;
        //        break;
        //    }
        //}

        // DO NOT ALLOW REPOSITIONING
        if (anchor)
            return;        

        // Show plane visualizer
        TogglePlaneVisualizer(true);

        // Raycast against the location the player touched to search for planes.        
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;
        //TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal | TrackableHitFlags.FeaturePoint | TrackableHitFlags.None | TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinInfinity;

        if (Frame.Raycast(Screen.width / 2f, Screen.height / 2f, raycastFilter, out hit))
        {
            UIController.Instance.ShowStatusText("Tap to place platform...", Color.cyan);
            platform.Material.color = Color.green;
            platform.Material.SetFloat("_OutlineAlpha", 1f);

            // Lerp to hit position
            var pos = hit.Pose.position;
            pos.y -= 0.5f;
            pos.z -= 1f;
            RoomPrefab.transform.position = Vector3.Lerp(RoomPrefab.transform.position, pos, Time.deltaTime * MOVE_SPEED);
            //RoomPrefab.transform.rotation = Quaternion.Lerp(RoomPrefab.transform.rotation, hit.Pose.rotation, Time.deltaTime * MOVE_SPEED);
            RoomPrefab.transform.rotation = Quaternion.identity;

            // Place room
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                return;

            UIController.Instance.ShowStatusText("", Color.red);
            platform.Material.SetFloat("_OutlineAlpha", 0f);
            GameController.Instance.Spawn(GameController.ModelTypes.GIRAFFE, true);

            // Look at camera but still be flush with the plane.
            //if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
            //{
            //    // Get the camera position and match the y-component with the hit position.
            //    Vector3 pos = FirstPersonCamera.transform.position;
            //    pos.y = RoomPrefab.transform.position.y;
            //    RoomPrefab.transform.LookAt(pos, RoomPrefab.transform.up);
            //}

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical world evolves.            
            anchor = hit.Trackable.CreateAnchor(hit.Pose);
            RoomPrefab.transform.parent = anchor.transform;

            // Hide plane visualizer
            TogglePlaneVisualizer(false);
        }
        else
        {
            UIController.Instance.ShowStatusText("Find a plane to place the platform!", Color.red);
            platform.Material.color = Color.red;
            platform.Material.SetFloat("_OutlineAlpha", 1f);

            // Position room 3 units in front of the camera
            var pos = FirstPersonCamera.transform.forward * roomStartPos.z;
            pos.y = FirstPersonCamera.transform.position.y + roomStartPos.y;

            RoomPrefab.transform.position = Vector3.Lerp(RoomPrefab.transform.position, pos, Time.deltaTime * MOVE_SPEED);
            //RoomPrefab.transform.rotation = Quaternion.Lerp(RoomPrefab.transform.rotation, hit.Pose.rotation, Time.deltaTime * MOVE_SPEED);
            RoomPrefab.transform.rotation = Quaternion.identity;
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

    private void _DoQuit()
    {
        Application.Quit();
    }

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
