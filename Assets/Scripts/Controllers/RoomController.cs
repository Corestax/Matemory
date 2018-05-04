using System.Collections;
using UnityEngine;

public class RoomController : Singleton<RoomController>
{
    [SerializeField]
    private GameObject BoundaryColliders;
    [SerializeField]
    private GameObject InnerBoundaryCollider;

    public Vector3 MaxDraggableBoundaries { get { return maxDraggableBoundaries; } }
    private Vector3 maxDraggableBoundaries;

    //public Vector3 MinDraggableBoundaries { get { return minDraggableBoundaries; } }
    //private Vector3 minDraggableBoundaries;

    public GameObject Room;

    void Start()
    {
        maxDraggableBoundaries = Vector3.zero;
        BoundaryColliders.SetActive(true);
        FindLocalBoundaries();
    }

    private void FindLocalBoundaries()
    {
        // Find max boundary positions for draggable objects to stay within local space
        var boundaries = BoundaryColliders.GetComponentsInChildren<Transform>();
        foreach (var b in boundaries)
        {
            if (b.transform.localPosition.x > maxDraggableBoundaries.x)
                maxDraggableBoundaries.x = b.transform.localPosition.x;
            if (b.transform.localPosition.y > maxDraggableBoundaries.y)
                maxDraggableBoundaries.y = b.transform.localPosition.y;
            if (b.transform.localPosition.z > maxDraggableBoundaries.z)
                maxDraggableBoundaries.z = b.transform.localPosition.z;
        }

        //minDraggableBoundaries = new Vector3(InnerBoundaryCollider.GetComponent<CapsuleCollider>().radius, InnerBoundaryCollider.GetComponent<CapsuleCollider>().radius, InnerBoundaryCollider.GetComponent<CapsuleCollider>().radius);
    }

    void OnEnable()
    {
        GameController.OnGameStarted += OnGameStarted;
    }

    void OnDisable()
    {

        GameController.OnGameStarted -= OnGameStarted;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ShowRoom(Vector3.zero);
    }

    public void ShowRoom(Vector3 pos)
    {
        Room.transform.position = pos;
        Room.SetActive(true);
    }    

    private void OnGameStarted(string statusText)
    {
        StartCoroutine(EnableInnerColliderTemporarily(2f));
    }

    private IEnumerator EnableInnerColliderTemporarily(float duration)
    {
        // Disable Outer Boundary Colliders to allow raycasting for grabbing items
        BoundaryColliders.SetActive(false);

        // Quickly toggle inner boundary colliders to disperse exploded items outwards
        EnableInnerBoundaryCollider();
        yield return new WaitForSeconds(duration);
        DisableInnerBoundaryCollider();
    }

    public void EnableInnerBoundaryCollider()
    {
        InnerBoundaryCollider.SetActive(true);
    }

    public void DisableInnerBoundaryCollider()
    {
        InnerBoundaryCollider.SetActive(false);
    }
}
