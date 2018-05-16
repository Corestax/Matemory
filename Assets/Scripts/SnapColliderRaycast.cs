using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class SnapColliderRaycast : MonoBehaviour
{
    [SerializeField]
    private LayerMask layer;
    //private int layerMask = 1 << 9;
    private Ray ray;
    private RaycastHit hit;
    private GameObject lastHitObject;
    private GameController gameController;
    private FruitRotatorController fruitRotatorController;
    private SnapController snapController;

    void Start()
    {
        gameController = GameController.Instance;
        fruitRotatorController = FruitRotatorController.Instance;
        snapController = SnapController.Instance;
    }

    void Update()
    {
        if (!gameController.IsGameRunning || !fruitRotatorController.SelectedFruit || (GameController.Instance.EnableAR && Input.touchCount == 0))
            return;

        RayCastSnapColliders();
    }

    private void RayCastSnapColliders()
    {
        if (GameController.Instance.EnableAR)
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        else
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 10f, layer))
        {
            // If new snap collider is hit
            if(hit.collider.gameObject != lastHitObject)
            {
                lastHitObject = hit.collider.gameObject;

                // Reset all colliders as idle
                SnapController.Instance.ResetAllColliderColorsToIdle();

                // Show hover color for snap collider
                SnapCollider snapCollider = hit.collider.GetComponent<SnapCollider>();
                SnapController.Instance.ActiveSnapCollider = snapCollider;
                snapCollider.ShowHover();
            }
        }
        else
        {
            if (lastHitObject)
            {
                lastHitObject.GetComponent<SnapCollider>().ShowIdle(true);
                lastHitObject = null;
            }
            snapController.ResetToIdle();
        }
    }
}
