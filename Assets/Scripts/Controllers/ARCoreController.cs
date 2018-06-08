using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.HelloAR;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARCoreController : MonoBehaviour
{
    [SerializeField]
    private Platform platform;

    public Camera CameraAR;
    public GameObject TrackedPlanePrefab;

    private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();
    //private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

    private LevelsController levelsController;
    private RoomController roomController;
    private UIController uiController;
    private TutorialsController tutorialController;
    private bool m_IsQuitting = false;
    private Anchor anchor;
    private bool isTrackingLost;
    private Vector3 roomStartPos;
    private TrackableHit hit;
    private GameObject room;

    private const float MOVE_SPEED = 2f;

    public static event Action OnTrackingActive;
    public static event Action OnTrackingLost;

    void Start()
    {
        levelsController = LevelsController.Instance;
        roomController = RoomController.Instance;
        uiController = UIController.Instance;
        tutorialController = TutorialsController.Instance;
        room = roomController.Room;

        // Set room
        room.transform.SetParent(CameraAR.transform);
        room.SetActive(true);
        roomStartPos = room.transform.position;
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
        if (anchor || EventSystem.current.IsPointerOverGameObject() || tutorialController.IsActive)
            return;        

        // Show plane visualizer
        TogglePlaneVisualizer(true);

        // Raycast against the location the player touched to search for planes.        
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal | TrackableHitFlags.FeaturePoint | TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinInfinity | TrackableHitFlags.PlaneWithinPolygon;

        if (Frame.Raycast(Screen.width / 2f, Screen.height / 2f, raycastFilter, out hit))
        {
            // Show green outline            
            uiController.ShowStatusText("Tap to place platform...", uiController.Color_statusText);
            platform.Material.color = Color.green;
            platform.Material.SetFloat("_OutlineAlpha", 1f);

            // Lerp to hit position (adjusted position to show at a good distance from camera)
            Vector3 pos = hit.Pose.position;

            // Move room
            room.transform.position = Vector3.MoveTowards(room.transform.position, pos, Time.deltaTime * MOVE_SPEED);
            room.transform.rotation = Quaternion.identity;

            // Place room
            if (Input.touchCount < 1 || (Input.GetTouch(0)).phase != TouchPhase.Began)
                return;

            uiController.ShowStatusText("", Color.red);
            platform.Material.SetFloat("_OutlineAlpha", 0f);
            LevelsController.Instance.LoadLastSavedLevel();

            // Look at camera
            if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
                roomController.Recenter();

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical world evolves.            
            anchor = hit.Trackable.CreateAnchor(hit.Pose);
            room.transform.parent = anchor.transform;

            // Hide plane visualizer
            TogglePlaneVisualizer(false);
        }
        else
        {
            // Show red outline
            if(!tutorialController.IsActive)
                uiController.ShowStatusText("Focus at a flat surface", Color.red);
            platform.Material.color = Color.red;
            platform.Material.SetFloat("_OutlineAlpha", 1f);

            // Position room in front of the camera
            var pos = CameraAR.transform.position + CameraAR.transform.forward * roomStartPos.z;

            // Move room
            room.transform.position = Vector3.MoveTowards(room.transform.position, pos, Time.deltaTime * MOVE_SPEED);
            room.transform.rotation = Quaternion.identity;
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
