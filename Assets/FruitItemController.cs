using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class FruitItemController : Singleton<FruitItemController>
{
    [SerializeField]
    private GameObject buttons_rotateItem;
    [SerializeField]
    private GameObject panel_controls;

    public enum ROTATIONS { NONE, X, Y, Z }
    private ROTATIONS activeRotation;

    private GameObject clonedFruitControl;
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
                break;

            case ROTATIONS.Y:
                if (isClockwise)
                    selectedFruit.transform.Rotate(Vector3.up * Time.deltaTime * SPEED, Space.World);
                else
                    selectedFruit.transform.Rotate(-Vector3.up, Time.deltaTime * SPEED, Space.World);
                break;

            case ROTATIONS.Z:
                if (isClockwise)
                    selectedFruit.transform.Rotate(Vector3.forward * Time.deltaTime * SPEED, Space.World);
                else
                    selectedFruit.transform.Rotate(-Vector3.forward, Time.deltaTime * SPEED, Space.World);
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

        // Clone fruit control object
        clonedFruitControl = Instantiate(selectedFruit.gameObject, panel_controls.transform, false);
        Destroy(clonedFruitControl.GetComponent<Rigidbody>());
        Destroy(clonedFruitControl.GetComponent<FruitItem>());
        Destroy(clonedFruitControl.GetComponent<Collider>());
        //clonedFruitControl = Instantiate(selectedFruit.GetComponentInChildren<MeshRenderer>().gameObject, panel_controls.transform, false);

        // Scale down
        clonedFruitControl.transform.localScale = new Vector3(clonedFruitControl.transform.localScale.x, clonedFruitControl.transform.localScale.y, clonedFruitControl.transform.localScale.z);

        //Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(clonedFruitControl.transform.position);
        //Vector2 WorldObject_ScreenPosition = new Vector2( ((ViewportPosition.x * rtCanvas.sizeDelta.x) - (rtCanvas.sizeDelta.x * 0.5f)), ((ViewportPosition.y * rtCanvas.sizeDelta.y) - (rtCanvas.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        //UI_Element.anchoredPosition = WorldObject_ScreenPosition;


        //print("panel_controls.transform.position: " + panel_controls.transform.localPosition);
        //Vector3 screenPos = panel_controls.GetComponent<RectTransform>().rect.position;
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rtTest, rtTest.position, Camera.main, out worldPos);
        clonedFruitControl.transform.position = worldPos;
        print(worldPos);
    }

    [SerializeField]
    private RectTransform rtTest;

    public void SetDropped()
    {
        if (!selectedFruit)
            return;

        selectedFruit.SetGrabbed(false);
        selectedFruit = null;
        buttons_rotateItem.SetActive(false);
        panel_controls.SetActive(true);

        // Destroy cloned fruit control object
        //if (clonedFruitControl)
        //    Destroy(clonedFruitControl);

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
