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
    private FruitItemController fruitItemController;
    private SnapController snapController;

    void Start()
    {
        fruitItemController = FruitItemController.Instance;
        snapController = SnapController.Instance;
    }

    void Update()
    {
        if (!fruitItemController.SelectedFruit)
            return;

        RayCastSnapColliders();
    }

    private void RayCastSnapColliders()
    {
        //Vector3 fwd = transform.TransformDirection(Vector3.forward);
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10f, layer))
        {
            // If new snap collider is hit
            if(hit.collider.gameObject != lastHitObject)
            {
                lastHitObject = hit.collider.gameObject;

                // Show all colliders as idle
                SnapController.Instance.ResetAllColliderColorsToIdle();

                // Show correct/incorrect
                SnapCollider snapCollider = hit.collider.GetComponent<SnapCollider>();
                if (fruitItemController.SelectedFruit.Fruit == snapCollider.FruitType)
                {
                    SnapController.Instance.ActiveSnapCollider = snapCollider;
                    snapCollider.ShowCorrect();
                }
                else
                {
                    snapCollider.ShowIncorrect();
                }
            }
        }
        else
        {
            if (SnapController.Instance.ActiveSnapCollider == this)
                SnapController.Instance.ActiveSnapCollider = null;
            if (lastHitObject)
            {
                lastHitObject.GetComponent<SnapCollider>().ShowIdle(true);
                lastHitObject = null;
            }
        }
    }
}
