﻿using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PinchZoom : MonoBehaviour
{
    [SerializeField]
    private Transform Platform;

    public Camera MainCamera { get; set; }

    private const float SPEED = 1f;

    private bool isAnimating;
    private bool isZoomIn;
    private float startDelta;
    private float scale;
    private Touch touch0;
    private Touch touch1;

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
            UIController.Instance.ShowConsoleText(touch0.phase + " -- " + touch1.phase);
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
        Vector3 targetPos =  new Vector3(MainCamera.transform.position.x, Platform.position.y, MainCamera.transform.position.z); 
        Vector3 distance = Platform.position - targetPos;
        Vector3 direction = distance.normalized;

        if(isZoomIn && distance.magnitude > 0.1f)
            Platform.position -= direction * SPEED * Time.deltaTime;
        else if (!isZoomIn && distance.magnitude < 1.5f)
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
