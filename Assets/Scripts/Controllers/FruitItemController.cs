using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class FruitItemController : Singleton<FruitItemController>
{
    [SerializeField]
    private Camera cameraFruitControl;
    [SerializeField]
    private GameObject panel_controls;

    public bool IsControlEnabled { get; private set; }

    private const float SPEED = 0.5f;

    private GameObject clonedFruit;
    private FruitItem selectedFruit;
    private Touch touch;

    void Start()
    {

    }
   
    void Update()
    {
        if (!IsControlEnabled)
            return;

        if (Input.touchCount == 2)
        {
            touch = Input.GetTouch(1);
            if (touch.phase == TouchPhase.Moved ||  touch.phase == TouchPhase.Stationary)
            {
                selectedFruit.transform.Rotate(touch.deltaPosition.y * SPEED, -touch.deltaPosition.x * SPEED, 0, Space.World);
                clonedFruit.transform.rotation = selectedFruit.transform.rotation;
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
        selectedFruit = item;
        selectedFruit.SetGrabbed(true);
        panel_controls.SetActive(true);
        cameraFruitControl.gameObject.SetActive(true);

        // Clone grabbed item (for render texture)
        clonedFruit = Instantiate(selectedFruit.gameObject, new Vector3(-100, -100, -100), selectedFruit.transform.rotation);
        clonedFruit.GetComponentInChildren<MeshRenderer>().material.SetFloat("_OutlineAlpha", 0f);
        Destroy(clonedFruit.GetComponent<Rigidbody>());
        Destroy(clonedFruit.GetComponent<FruitItem>());
        Destroy(clonedFruit.GetComponent<Collider>());

        // Set render texture camera to look at cloned item
        Vector3 camPos = clonedFruit.transform.position;
        camPos.z -= 0.3f;
        cameraFruitControl.transform.position = camPos;
        cameraFruitControl.transform.LookAt(clonedFruit.transform.position);
    }

    public void SetDropped()
    {
        if (!selectedFruit)
            return;

        // Drop item
        selectedFruit.SetGrabbed(false);
        selectedFruit = null;

        // Disable control
        panel_controls.SetActive(false);
        cameraFruitControl.gameObject.SetActive(false);
        cameraFruitControl.transform.SetParent(transform.root.parent);

        // Destroy cloned item
        if (clonedFruit)
            Destroy(clonedFruit);
    }    
}
