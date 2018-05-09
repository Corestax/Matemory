using UnityEngine;

[DisallowMultipleComponent]
public class FruitItem : MonoBehaviour
{
    public enum FruitTypes { APPLE, APPLE_HALF, APPLE_WEDGE, BANANA, BROCCOLI, CARAMBOLA, CARAMBOLA_SLICE, CELERY, GRAPE, ORANGE, ORANGE_HALF, ORANGE_WEDGE, PEAR, PEAR_HALF, PEAR_WEDGE, PINEAPPLE, STRAWBERRY, WATERMELON, WATERMELON_HALF, WATERMELON_SLICE , CUSTOM1, CUSTOM2, CUSTOM3, CUSTOM4, CUSTOM5 }
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

//#if UNITY_EDITOR
//        material.shader = Shader.Find("Mobile/Diffuse");
//#endif
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
        // Offset X
        float padding = fruitCollider.bounds.size.x / 4f;
        float offset = roomController.Room.transform.position.x;
        float x = Mathf.Clamp(transform.position.x, -roomController.MaxDraggableBoundaries.x + padding + offset, roomController.MaxDraggableBoundaries.x - padding + offset);

        // Offset Y
        padding = fruitCollider.bounds.size.y / 2f;
        offset = roomController.PlatePlane.transform.position.y;
        float y = Mathf.Clamp(transform.position.y, padding + offset, roomController.MaxDraggableBoundaries.y - padding + offset);

        // Offset Z
        offset = roomController.Room.transform.position.z;
        padding = fruitCollider.bounds.size.z / 4f;
        float z = Mathf.Clamp(transform.position.z, -roomController.MaxDraggableBoundaries.z + padding + offset, roomController.MaxDraggableBoundaries.z - padding + offset);

        // Set position
        transform.position = new Vector3(x, y, z);
    }           

    public void Explode()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        rigidBody.AddRelativeForce(Random.onUnitSphere * explosionForce, ForceMode.Impulse);
    }

    public void SetGrabbed()
    {
        material.SetFloat("_OutlineAlpha", 1f);
        rigidBody.isKinematic = true;
        SnapController.Instance.EnableColliders();
    }

    public void SetDropped()
    {
        material.SetFloat("_OutlineAlpha", 0f);
        rigidBody.isKinematic = false;
        SnapController.Instance.DisableColliders();

        // Snap
        if (SnapController.Instance.ActiveSnapCollider && SnapController.Instance.ActiveSnapCollider.FruitType == Fruit)
            SnapController.Instance.Snap(this);

        SnapController.Instance.ClearSnapColliders();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Wall")
            AudioManager.Instance.PlayCollisionSound();
    }
}
