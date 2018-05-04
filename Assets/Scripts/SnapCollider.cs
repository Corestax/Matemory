using UnityEngine;

public class SnapCollider : MonoBehaviour
{
    public FruitItem.FruitTypes FruitType;
    public int[] Order;
    public Vector3 PositionToSnap;
    public Quaternion RotiationToSnap;

    public bool IsSnapped { get; private set; }

    private Material material;
    private Color colorIdle;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Clone material
        material = meshRenderer.material;
        material.CopyPropertiesFromMaterial(new Material(material));
        colorIdle = material.GetColor("_GridColor");
    }

    private void OnEnable()
    {
        GameController.OnGameStarted += Show;
        GameController.OnGameEnded += Hide;
    }

    private void OnDisable()
    {
        GameController.OnGameStarted -= Show;
        GameController.OnGameEnded -= Hide;
    }

    private void Show(string statusText)
    {
        meshRenderer.enabled = true;
    }

    private void Hide(string statusText)
    {
        meshRenderer.enabled = false;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsSnapped)
            return;

        if (other.tag == "Draggable")
        {
            // Show all colliders as idle
            SnapController.Instance.ResetAllColliderColorsToIdle();

            // Show correct/incorrect
            var fruitItem = other.GetComponent<FruitItem>();
            if (fruitItem.Fruit == FruitType)
            {
                SnapController.Instance.ActiveSnapCollider = this;                
                ShowCorrect();
            }
            else
            {
                ShowIncorrect();
            }
        }
    }    

    private void OnTriggerExit(Collider other)
    {
        if(SnapController.Instance.ActiveSnapCollider == this)
            SnapController.Instance.ActiveSnapCollider = null;
        ShowIdle();
    }

    public void ShowCorrect()
    {
        material.SetColor("_GridColor", Color.green);
    }

    public void ShowIncorrect()
    {
        material.SetColor("_GridColor", Color.red);
    }

    public void ShowIdle()
    {
        material.SetColor("_GridColor", colorIdle);
    }

    public void Snap()
    {
        IsSnapped = true;
        meshRenderer.enabled = false;        
    }        
}
