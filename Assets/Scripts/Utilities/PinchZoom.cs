using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PinchZoom : MonoBehaviour
{
    [SerializeField]
    private Transform Platform;

    public Camera MainCamera { get; set; }

    private const float SPEED = 2f;
    private const float MAX_ZOOM_IN = 0.25f;
    private const float MAX_ZOOM_OUT = 1.25f;

    private bool isAnimating;
    private bool isZoomIn;
    private float startDelta;
    private float scale;
    private Touch touch0;
    private Touch touch1;

    private const float OFFSET_Y = 0.25f;

    void Start()
    {
        scale = 1f;
    }
    
    void Update()
    {                 
        if (Input.touches.Length == 2)
        {
            if (FruitItemController.Instance.IsControlEnabled)
                return;

            touch0 = Input.touches[0];
            touch1 = Input.touches[1];

            if (touch1.phase == TouchPhase.Began)
            {
                startDelta = Vector2.Distance(touch0.position, touch1.position);
            }
            else
            {
                float currDelta = Vector2.Distance(touch0.position, touch1.position);
                float currScale = currDelta / startDelta;

                if (scale > currScale)
                    Zoom(false);
                else if (scale < currScale)
                    Zoom(true);

                scale = currScale;

                if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
                    StopZoom();
            }            
        }

        if (!isAnimating)
            return;

        // Zoom in/out (move platform towards player camera - ignore y axis)
        Vector3 targetPos =  new Vector3(MainCamera.transform.position.x, Mathf.Clamp(MainCamera.transform.position.y, Platform.transform.position.y, MainCamera.transform.position.y - OFFSET_Y), MainCamera.transform.position.z); 
        Vector3 distance = Platform.position - targetPos;
        Vector3 direction = distance.normalized;

        if(isZoomIn && distance.magnitude > MAX_ZOOM_IN)
            Platform.position -= direction * SPEED * Time.deltaTime;
        else if (!isZoomIn && distance.magnitude < MAX_ZOOM_OUT)
            Platform.position += direction * SPEED * Time.deltaTime;        
    }

    public void Zoom(bool zoomIn)
    {
        isAnimating = true;
        isZoomIn = zoomIn;
    }

    public void StopZoom()
    {
        isAnimating = false;
    }
}
