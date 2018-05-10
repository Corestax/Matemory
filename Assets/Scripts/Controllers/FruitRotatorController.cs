using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class FruitRotatorController : Singleton<FruitRotatorController>
{
    [SerializeField]
    private Camera cameraFruitControl;
    [SerializeField]
    private GameObject panel_controls;

    public bool IsControlEnabled { get; private set; }
    public FruitItem SelectedFruit { get; private set; }

    private const float SPEED = 0.5f;

#if UNITY_EDITOR
    public enum ROTATIONS { NONE, X, Y, Z }
    private ROTATIONS activeRotation;
    private bool isClockwise;
#endif

    private GameController gameController;
    private GameObject clonedFruit;
    private Touch touch;

    void Start()
    {
        gameController = GameController.Instance;

#if UNITY_EDITOR
        activeRotation = ROTATIONS.NONE;
#endif
    }

    private void OnEnable()
    {
        ARCoreController.OnTrackingActive += ShowControls;
        ARCoreController.OnTrackingLost += HideControls;
    }

    private void OnDisable()
    {
        ARCoreController.OnTrackingActive -= ShowControls;
        ARCoreController.OnTrackingLost -= HideControls;
    }

    private void ShowControls()
    {
        if (!SelectedFruit)
            return;

        panel_controls.SetActive(true);
        cameraFruitControl.gameObject.SetActive(true);
    }

    private void HideControls()
    {
        panel_controls.SetActive(false);
        cameraFruitControl.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameController.IsGameRunning)
            return;

#if UNITY_EDITOR
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
            Rotate(ROTATIONS.Y, true);
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            Rotate(ROTATIONS.Y, false);
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
            Rotate(ROTATIONS.X, true);
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
            Rotate(ROTATIONS.X, false);
        if (UnityEngine.Input.GetKeyDown(KeyCode.Period))
            Rotate(ROTATIONS.Z, true);
        if (UnityEngine.Input.GetKeyDown(KeyCode.Comma))
            Rotate(ROTATIONS.Z, false);
        if (UnityEngine.Input.GetKeyUp(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyUp(KeyCode.RightArrow) || UnityEngine.Input.GetKeyUp(KeyCode.UpArrow) || UnityEngine.Input.GetKeyUp(KeyCode.DownArrow) || UnityEngine.Input.GetKeyUp(KeyCode.Period) || UnityEngine.Input.GetKeyUp(KeyCode.Comma))
            StopRotate();

        if (!SelectedFruit || activeRotation == ROTATIONS.NONE)
            return;

        switch (activeRotation)
        {
            case ROTATIONS.NONE:
                break;

            case ROTATIONS.X:
                if (isClockwise)
                    SelectedFruit.transform.Rotate(Vector3.right * Time.deltaTime * 100f, Space.World);
                else
                    SelectedFruit.transform.Rotate(-Vector3.right, Time.deltaTime * 100f, Space.World);
                clonedFruit.transform.rotation = SelectedFruit.transform.rotation;
                break;

            case ROTATIONS.Y:
                if (isClockwise)
                    SelectedFruit.transform.Rotate(Vector3.up * Time.deltaTime * 100f, Space.World);
                else
                    SelectedFruit.transform.Rotate(-Vector3.up, Time.deltaTime * 100f, Space.World);
                clonedFruit.transform.rotation = SelectedFruit.transform.rotation;
                break;

            case ROTATIONS.Z:
                if (isClockwise)
                    SelectedFruit.transform.Rotate(Vector3.forward * Time.deltaTime * 100f, Space.World);
                else
                    SelectedFruit.transform.Rotate(-Vector3.forward, Time.deltaTime * 100f, Space.World);
                clonedFruit.transform.rotation = SelectedFruit.transform.rotation;
                break;

            default:
                break;
        }
#endif


        if (!IsControlEnabled)
            return;

        if (Input.touchCount == 2)
        {
            touch = Input.GetTouch(1);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                SelectedFruit.transform.Rotate(touch.deltaPosition.y * SPEED, -touch.deltaPosition.x * SPEED, 0, Space.World);
                clonedFruit.transform.rotation = SelectedFruit.transform.rotation;
            }
        }
    }

    // Controls are enabled only when finger is touching controls panel
    public void EnableControls()
    {
        IsControlEnabled = true;
    }

    public void DisableControls()
    {
        IsControlEnabled = false;
    }

    public void SetGrabbed(FruitItem item)
    {
        SelectedFruit = item;
        SelectedFruit.SetGrabbed();
        ShowControls();

        // Clone grabbed item (for render texture)
        clonedFruit = Instantiate(SelectedFruit.gameObject, new Vector3(-100, -100, -100), SelectedFruit.transform.rotation);
        clonedFruit.GetComponentInChildren<MeshRenderer>().material.SetFloat("_OutlineAlpha", 0f);
        Destroy(clonedFruit.GetComponent<Rigidbody>());
        Destroy(clonedFruit.GetComponent<FruitItem>());
        Destroy(clonedFruit.GetComponent<Collider>());

        // Set render texture camera to look at cloned item
        Vector3 camPos = clonedFruit.transform.position;

        // Find largest side
        float distance = clonedFruit.GetComponent<Collider>().bounds.size.x;
        if (clonedFruit.GetComponent<Collider>().bounds.size.y > distance)
            distance = clonedFruit.GetComponent<Collider>().bounds.size.y;
        if (clonedFruit.GetComponent<Collider>().bounds.size.z > distance)
            distance = clonedFruit.GetComponent<Collider>().bounds.size.z;

        // Set camera to look at cloned item
        camPos.z -= distance;
        cameraFruitControl.transform.position = camPos;
        cameraFruitControl.transform.LookAt(clonedFruit.transform.position);
    }

    public void SetDropped()
    {
        if (!SelectedFruit)
            return;

        // Drop item
        SelectedFruit.SetDropped();
        SelectedFruit = null;

        // Disable control
        HideControls();

        // Destroy cloned item
        if (clonedFruit)
            Destroy(clonedFruit);
    }

#if UNITY_EDITOR
    public void Rotate(ROTATIONS rotation, bool clockwise)
    {
        if (!SelectedFruit)
            return;

        activeRotation = rotation;
        isClockwise = clockwise;
    }

    public void StopRotate()
    {
        activeRotation = ROTATIONS.NONE;
    }
#endif
}
