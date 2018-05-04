using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapController : Singleton<SnapController>
{
    [SerializeField]
    private GameObject Prefab_SnapCollider;
    [SerializeField]
    private Transform SnapColliders;

    public SnapCollider ActiveSnapCollider { get; set; }

    private List<Collider> Colliders;

	void Start ()
    {
        Colliders = new List<Collider>();
    }

    public void CreateSnapCollider(FruitItem fruitItem, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(Prefab_SnapCollider, pos, rot, SnapColliders);
        var snapCollider = go.GetComponent<SnapCollider>();
        Colliders.Add(snapCollider.GetComponent<Collider>());

        // Copy data to collider
        snapCollider.FruitType = fruitItem.Fruit;
        snapCollider.PositionToSnap = fruitItem.PositionToSnap;
        snapCollider.RotiationToSnap = fruitItem.RotationToSnap;
        snapCollider.Order = fruitItem.Order;
    }

    public void EnableColliders()
    {
        foreach (var c in Colliders)
        {
            if(!c.GetComponent<SnapCollider>().IsTaken)
                c.enabled = true;
        }
    }

    public void DisableColliders()
    {
        foreach (var c in Colliders)
            c.enabled = false;
    }

    public void ClearSnapColliders()
    {
        foreach(var c in Colliders)
            Destroy(c.gameObject);

        Colliders.Clear();
        ActiveSnapCollider = null;
    }
}
