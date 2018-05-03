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
    private GameObject buttons_rotateItem;
    [SerializeField]
    private GameObject panel_controls;

    public enum ROTATIONS { NONE, X, Y, Z }
    private ROTATIONS activeRotation;

    private GameObject clonedFruit;
    private FruitItem selectedFruit;
    private bool isClockwise;
    private const float SPEED = 100f;

    void Start()
    {
        activeRotation = ROTATIONS.NONE;
    }
   
    void Update()
    {
        //if(Input.touchCount > 2)
        //{
        //    if(touch)
        //}


        if (Input.GetKey(KeyCode.LeftArrow))
            Rotate(ROTATIONS.Y, true);
        else if (Input.GetKey(KeyCode.RightArrow))
            Rotate(ROTATIONS.Y, false);
        if (Input.GetKey(KeyCode.UpArrow))
            Rotate(ROTATIONS.X, true);
        else if (Input.GetKey(KeyCode.DownArrow))
            Rotate(ROTATIONS.X, false);
        if (Input.GetKey(KeyCode.Period))
            Rotate(ROTATIONS.Z, true);
        else if (Input.GetKey(KeyCode.Comma))
            Rotate(ROTATIONS.Z, false);

        if (!selectedFruit || activeRotation == ROTATIONS.NONE)
            return;

        switch (activeRotation)
        {
            case ROTATIONS.NONE:
                break;

            case ROTATIONS.X:
                if (isClockwise)
                    selectedFruit.transform.Rotate(Vector3.right * Time.deltaTime * SPEED, Space.World);
                else
                    selectedFruit.transform.Rotate(-Vector3.right, Time.deltaTime * SPEED, Space.World);
                clonedFruit.transform.rotation = selectedFruit.transform.rotation;
                break;

            case ROTATIONS.Y:
                if (isClockwise)
                    selectedFruit.transform.Rotate(Vector3.up * Time.deltaTime * SPEED, Space.World);
                else
                    selectedFruit.transform.Rotate(-Vector3.up, Time.deltaTime * SPEED, Space.World);
                clonedFruit.transform.rotation = selectedFruit.transform.rotation;
                break;

            case ROTATIONS.Z:
                if (isClockwise)
                    selectedFruit.transform.Rotate(Vector3.forward * Time.deltaTime * SPEED, Space.World);
                else
                    selectedFruit.transform.Rotate(-Vector3.forward, Time.deltaTime * SPEED, Space.World);
                clonedFruit.transform.rotation = selectedFruit.transform.rotation;
                break;

            default:
                break;
        }        
    }

    public void SetGrabbed(FruitItem item)
    {
        selectedFruit = item;
        selectedFruit.SetGrabbed(true);
        buttons_rotateItem.SetActive(true);
        panel_controls.SetActive(true);
        cameraFruitControl.gameObject.SetActive(true);

        // Clone grabbed fruit for render texture
        clonedFruit = Instantiate(selectedFruit.gameObject, new Vector3(-100, -100, -100), selectedFruit.transform.rotation);
        Destroy(clonedFruit.GetComponent<Rigidbody>());
        Destroy(clonedFruit.GetComponent<FruitItem>());
        Destroy(clonedFruit.GetComponent<Collider>());

        // Set render texture camera to look at cloned fruit
        Vector3 camPos = clonedFruit.transform.position;
        camPos.z -= 0.3f;
        cameraFruitControl.transform.position = camPos;
        cameraFruitControl.transform.LookAt(clonedFruit.transform.position);
    }

    public void SetDropped()
    {
        if (!selectedFruit)
            return;

        selectedFruit.SetGrabbed(false);
        selectedFruit = null;
        buttons_rotateItem.SetActive(false);
        panel_controls.SetActive(false);
        cameraFruitControl.gameObject.SetActive(false);
        cameraFruitControl.transform.SetParent(transform.root.parent);

        // Destroy cloned fruit
        if (clonedFruit)
            Destroy(clonedFruit);

        StopRotate();
    }

    public void Rotate(bool clockwise)
    {
        if (!selectedFruit)
            return;

        isClockwise = clockwise;
    }

    public void Rotate(ROTATIONS rotation, bool clockwise)
    {
        if (!selectedFruit)
            return;

        activeRotation = rotation;
        isClockwise = clockwise;
    }

    public void StopRotate()
    {
        activeRotation = ROTATIONS.NONE;
    }
}
