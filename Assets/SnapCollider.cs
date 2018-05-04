using UnityEngine;

public class SnapCollider : MonoBehaviour
{
    public FruitItem.FruitTypes FruitType;
    public int[] Order;
    public Vector3 PositionToSnap;
    public Quaternion RotiationToSnap;

    public bool IsTaken { get; private set; }

    private Material material;
    private Color colorIdle;

    private void Start()
    {
        // Clone material
        material = GetComponent<MeshRenderer>().material;
        material.CopyPropertiesFromMaterial(new Material(material));
        colorIdle = material.GetColor("_GridColor");
    }    

    private void OnTriggerEnter(Collider other)
    {
        if (IsTaken)
            return;

        if (other.tag == "Draggable")
        {
            var fruitItem = other.GetComponent<FruitItem>();
            if (fruitItem.Fruit == FruitType)
            {
                SnapController.Instance.ActiveSnapCollider = this;
                ShowHighlight();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SnapController.Instance.ActiveSnapCollider = null;
        HideHighlight();
    }

    public void SetTaken()
    {
        IsTaken = true;
        ShowHighlightCorrect();
    }

    private void ShowHighlight()
    {
        material.SetColor("_GridColor", Color.red);
    }

    private void HideHighlight()
    {
        material.SetColor("_GridColor", colorIdle);
    }

    private void ShowHighlightCorrect()
    {
        material.SetColor("_GridColor", Color.green);
    }
}
