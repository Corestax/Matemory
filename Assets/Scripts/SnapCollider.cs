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
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
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

    private void Show()
    {
        meshRenderer.enabled = true;
    }

    private void Hide(GameController.EndGameTypes _type)
    {
        meshRenderer.enabled = false;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (IsSnapped)
    //        return;

    //    if (other.tag == "Draggable")
    //    {
    //        // Show all colliders as idle
    //        SnapController.Instance.ResetAllColliderColorsToIdle();

    //        // Show correct/incorrect
    //        var fruitItem = other.GetComponent<FruitItem>();
    //        if (fruitItem.Fruit == FruitType)
    //        {
    //            SnapController.Instance.ActiveSnapCollider = this;
    //            ShowCorrect();
    //        }
    //        else
    //        {
    //            ShowIncorrect();
    //        }
    //    }
    //}

    //public void OnTriggerEnter()
    //{
        
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if(SnapController.Instance.ActiveSnapCollider == this)
    //        SnapController.Instance.ActiveSnapCollider = null;
    //    ShowIdle(true);
    //}

    public void ShowCorrect()
    {
        material.SetColor("_GridColor", Color.green);
        audioManager.PlaySound(audioManager.audio_snapCorrect);
    }

    public void ShowIncorrect()
    {
        material.SetColor("_GridColor", Color.red);
        audioManager.PlaySound(audioManager.audio_snapIncorrect);
    }

    public void ShowIdle(bool playSound)
    {
        material.SetColor("_GridColor", colorIdle);
        if(playSound)
            audioManager.PlaySound(audioManager.audio_snapIdle);
    }

    public void Snap()
    {
        IsSnapped = true;
        meshRenderer.enabled = false;
        audioManager.PlaySound(audioManager.audio_snapComplete);
    }        
}
