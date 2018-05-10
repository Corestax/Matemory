using UnityEngine;

[DisallowMultipleComponent]
public class FruitItem : MonoBehaviour
{
    public enum FruitTypes { APPLE = 1, APPLE_HALF = 20, APPLE_WEDGE = 40, BANANA = 60, BROCCOLI = 80, CARAMBOLA = 100, CARAMBOLA_SLICE = 120, CELERY = 140, CELERY_SHORT = 160, CUCUMBER = 180, CUCUMBER_SLICE = 200, GRAPE = 220, ORANGE = 240, ORANGE_HALF = 260, ORANGE_WEDGE = 280, PEAR = 300, PEAR_HALF = 320, PEAR_WEDGE = 340, PINEAPPLE = 360, STRAWBERRY = 380, WATERMELON = 400, WATERMELON_HALF = 420, WATERMELON_SLICE = 440, CUSTOM1 = 460, CUSTOM2 = 480, CUSTOM3 = 500, CUSTOM4 = 520, CUSTOM5 = 540 }
    public FruitTypes Fruit;

    public int[] Order;
    public Vector3 PositionToSnap { get; private set; }
    public Quaternion RotationToSnap { get; private set; }
    public bool IsSnapped { get; set; }

    private RoomController roomController;
    private Rigidbody rigidBody;
    private Collider fruitCollider;
    private Material material;
    private float explosionForce;

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

        SnapController.Instance.CreateSnapCollider(this, transform.position, transform.rotation);

        // Clone material
        material = GetComponentInChildren<MeshRenderer>().material;
        material.CopyPropertiesFromMaterial(new Material(material));

//#if UNITY_EDITOR
//        material.shader = Shader.Find("Mobile/Diffuse");
//#endif
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

        SnapController.Instance.ResetToIdle();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Wall")
            AudioManager.Instance.PlayCollisionSound();
    }
}
