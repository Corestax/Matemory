using UnityEngine;

public class SnapCollider : MonoBehaviour
{
    public FruitItem.FruitTypes FruitType;
    public int[] Order;
    public Vector3 PositionToSnap;
    public Quaternion RotiationToSnap;

    public bool IsTaken { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTaken)
            return;

        if (other.tag == "Draggable")
        {
            var fruitItem = other.GetComponent<FruitItem>();
            if (fruitItem.Fruit == FruitType)
                SnapController.Instance.ActiveSnapCollider = this;                
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SnapController.Instance.ActiveSnapCollider = null;
    }
}
