﻿using UnityEngine;

[DisallowMultipleComponent]
public class FruitItem : MonoBehaviour
{
    public enum FruitTypes { PINEAPPLE, BANNANA, WATERMELON }
    public FruitTypes Fruit;

    public int[] Order;
    public Vector3 PositionToSnap { get; private set; }
    public Quaternion RotationToSnap { get; private set; }

    private RoomController roomController;
    private Rigidbody rigidBody;
    private Collider fruitCollider;
    private Material material;
    private float explosionForce;
    private bool showStartPosition;
    private Vector3 posVisualIndicator;

    void Start()
    {
        roomController = RoomController.Instance;
        rigidBody = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
        rigidBody.isKinematic = true;
        explosionForce = 10f;

        // Cache start values
        PositionToSnap = transform.localPosition;
        RotationToSnap = transform.localRotation;
        posVisualIndicator = transform.position;

        SnapController.Instance.CreateSnapCollider(this, transform.position, transform.rotation);

        // Clone material
        material = GetComponentInChildren<MeshRenderer>().material;
        material.CopyPropertiesFromMaterial(new Material(material));

#if UNITY_EDITOR
        material.shader = Shader.Find("Mobile/Diffuse");
#endif
    }

    void OnEnable()
    {
        CheckShowStartPosition();
        if (showStartPosition)
            GameController.OnGameStarted += OnGameStarted;
    }

    void OnDisable()
    {
        if (showStartPosition)
            GameController.OnGameStarted -= OnGameStarted;
    }

    private void CheckShowStartPosition()
    {
        foreach (int i in Order)
        {
            if (i == 1)
            {
                showStartPosition = true;
                return;
            }
        }
    }

    private void OnGameStarted()
    {
        HighlightStartPositions();
    }

    private void HighlightStartPositions()
    {
        if (!showStartPosition)
            return;

        VisualIndicatorController.Instance.ShowIndicator(this, posVisualIndicator);
    }    

    void Update()
    {
        // Constaint positions within boundaries
        StayWithinBoundaries();
    }    

    private void StayWithinBoundaries()
    {
        // Constaint positions within boundaries (Allow dragging to origin)
        //if (isGrabbed || !GameController.Instance.IsGameRunning)
        //{
            // Offset X
            float padding = fruitCollider.bounds.size.x / 4f;
            float offset = roomController.Room.transform.position.x;
            float x = Mathf.Clamp(transform.position.x, -roomController.MaxDraggableBoundaries.x + padding + offset, roomController.MaxDraggableBoundaries.x - padding + offset);

            // Offset Y
            float minHeight = fruitCollider.bounds.size.y / 4f;
            padding = fruitCollider.bounds.size.y / 2f;
            offset = roomController.Room.transform.position.y;
            float y = Mathf.Clamp(transform.position.y, minHeight + padding + offset, roomController.MaxDraggableBoundaries.y - padding + offset);

            // Offset Z
            offset = roomController.Room.transform.position.z;
            padding = fruitCollider.bounds.size.z / 4f;
            float z = Mathf.Clamp(transform.position.z, -roomController.MaxDraggableBoundaries.z + padding + offset, roomController.MaxDraggableBoundaries.z - padding + offset);

            // Set position
            transform.position = new Vector3(x, y, z);
        //}
        //// Constaint positions within boundaries (Do not allow dragging to origin)
        //else
        //{
        //    // Offset X
        //    float padding = fruitCollider.bounds.size.x / 4f;
        //    float x;
        //    float offset = roomController.Room.transform.position.x;
        //    if (transform.position.x < offset)
        //        x = Mathf.Clamp(transform.position.x, -roomController.MaxDraggableBoundaries.x + padding + offset, -roomController.MinDraggableBoundaries.x - padding + offset);
        //    else
        //        x = Mathf.Clamp(transform.position.x, roomController.MinDraggableBoundaries.x + padding + offset, roomController.MaxDraggableBoundaries.x - padding + offset);

        //    // Offset Y
        //    float minHeight = fruitCollider.bounds.size.y / 2f;
        //    padding = fruitCollider.bounds.size.y / 4f;
        //    offset = roomController.Room.transform.position.y;
        //    float y = Mathf.Clamp(transform.position.y, minHeight + padding + offset, roomController.MaxDraggableBoundaries.y - padding + offset);

        //    // Offset Z
        //    float z;
        //    padding = fruitCollider.bounds.size.z / 4f;
        //    offset = roomController.Room.transform.position.z;
        //    if (transform.position.z < offset)
        //        z = Mathf.Clamp(transform.position.z, -roomController.MaxDraggableBoundaries.z + padding + offset, -roomController.MinDraggableBoundaries.z - padding + offset);
        //    else
        //        z = Mathf.Clamp(transform.position.z, roomController.MinDraggableBoundaries.z + padding + offset, roomController.MaxDraggableBoundaries.z - padding + offset);

        //    // Set position
        //    transform.position = new Vector3(x, y, z);
        //}
    }           

    public void Explode()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        rigidBody.AddRelativeForce(Random.onUnitSphere * explosionForce, ForceMode.Impulse);
    }

    public void SetGrabbed(bool state)
    {
        // Outline
        if (state)
            material.SetFloat("_OutlineAlpha", 1f);
        else
            material.SetFloat("_OutlineAlpha", 0f);

        if(state)
            SetLayerRecursively(gameObject, 8);
        else
            SetLayerRecursively(gameObject, 0);
        rigidBody.isKinematic = state;

        // Check distance to snap
        if (state)
        {
            SnapController.Instance.EnableColliders();
        }
        else
        {
            SnapController.Instance.DisableColliders();
            SnapController.Instance.Snap(this);
        }
    }

    public static void SetLayerRecursively(GameObject go, int layerNumber)
    {
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = layerNumber;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Wall")
            AudioManager.Instance.PlayCollisionSound();
    }
}
