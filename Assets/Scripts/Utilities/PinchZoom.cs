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

    private bool isAnimating;
    private bool isZoomIn;
    private float speed;

    private float startDelta;
    private float scale;

    void Start()
    {
        speed = 1f;
        scale = 1f;
    }

    void Update()
    {
        if (!isAnimating)
            return;

        // Find position to move to 
        Vector3 targetPos =  new Vector3(MainCamera.transform.position.x, Platform.position.y, MainCamera.transform.position.z); 
        float step = speed * Time.deltaTime;

        if (isZoomIn)
            Platform.position =  Vector3.MoveTowards(Platform.position, targetPos, step);
        else
            Platform.position = Vector3.MoveTowards(Platform.position, -targetPos, step);

        //if (Input.touches.Length == 2)
        //{
        //    print("2 TOUCHES!" + Input.GetTouch(0).phase + " -- " + Input.GetTouch(1).phase);
        //    if (Input.GetTouch(1).phase == TouchPhase.Began)
        //    {
        //        startDelta = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
        //    }
        //    else
        //    {
        //        float currDelta = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
        //        float currScale = currDelta / startDelta;

        //        if (scale > currScale)
        //            Zoom(true);
        //        else if (scale < currScale)
        //            Zoom(false);

        //        scale = currScale;
        //    }
        //}
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
