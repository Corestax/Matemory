using System.Collections.Generic;
using UnityEngine;

public class SnapController : Singleton<SnapController>
{
    [SerializeField]
    private GameObject Prefab_SnapCollider;
    [SerializeField]
    private Transform Container;

    public SnapCollider ActiveSnapCollider { get; set; }

    private List<SnapCollider> SnapColliders;

	void Start ()
    {
        SnapColliders = new List<SnapCollider>();
    }

    public void CreateSnapCollider(FruitItem fruitItem, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(Prefab_SnapCollider, pos, rot, Container);
        var snapCollider = go.GetComponent<SnapCollider>();
        SnapColliders.Add(snapCollider);

        // Copy data to collider
        snapCollider.FruitType = fruitItem.Fruit;
        snapCollider.PositionToSnap = fruitItem.PositionToSnap;
        snapCollider.RotiationToSnap = fruitItem.RotationToSnap;
        snapCollider.Order = fruitItem.Order;
    }

    public void Snap(FruitItem fruitItem)
    {
        if (!ActiveSnapCollider || (ActiveSnapCollider && ActiveSnapCollider.IsSnapped))
            return;

        // Mark collider as snapped
        ActiveSnapCollider.Snap();

        // Snap to position
        fruitItem.GetComponent<Rigidbody>().isKinematic = true;
        fruitItem.transform.localPosition = ActiveSnapCollider.PositionToSnap;
        fruitItem.transform.localRotation = ActiveSnapCollider.RotiationToSnap;
        fruitItem.tag = "Untagged";

        // Check if game won
        CheckIfAllSnapped();
    }

    public void CheckIfAllSnapped()
    {
        int count = 0;
        foreach(var sc in SnapColliders)
        {
            if (sc.IsSnapped)
                count++;
        }

        // If game won
        if (SnapColliders.Count == count)
            GameController.Instance.StopGame("COMPLETE!");
    }

    public void ResetAllColliderColorsToIdle()
    {
        foreach(var sc in SnapColliders)
            sc.ShowIdle();
    }

    public void EnableColliders()
    {
        foreach (var sc in SnapColliders)
        {
            if(!sc.IsSnapped)
                sc.GetComponent<Collider>().enabled = true;
        }
    }

    public void DisableColliders()
    {
        foreach (var sc in SnapColliders)
            sc.GetComponent<Collider>().enabled = false;
    }

    public void ClearSnapColliders()
    {
        foreach(var sc in SnapColliders)
            Destroy(sc.gameObject);

        SnapColliders.Clear();
        ActiveSnapCollider = null;
    }
}
