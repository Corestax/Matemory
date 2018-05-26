using UnityEngine;
using System.Collections;

public class SnapCollider : MonoBehaviour
{
    public FruitsController.FruitTypes FruitType;
    public FruitsController.FruitTypes FruitSnapped { get; private set; }
    //public int[] Order;
    public Vector3 PositionToSnap;
    public Quaternion RotiationToSnap;
    public float Size;

    public bool IsSnapped { get; private set; }

    private AudioManager audioManager;
    private UIController uiController;
    private MeshRenderer meshRenderer;
    private Material material;
    private Color colorIdle;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        uiController = UIController.Instance;
        meshRenderer = GetComponent<MeshRenderer>();

        // Clone material
        material = meshRenderer.material;
        material.CopyPropertiesFromMaterial(new Material(material));
        colorIdle = material.GetColor("_Color");
    }

    private void OnEnable()
    {
        GameController.OnGameStarted += OnGameStarted;
        GameController.OnGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameController.OnGameStarted -= OnGameStarted;
        GameController.OnGameEnded -= OnGameEnded;
    }    

    private void OnGameStarted()
    {
        meshRenderer.enabled = true;  
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        meshRenderer.enabled = false;
    }

    public void ShowIdle(bool playSound)
    {
        material.SetColor("_Color", colorIdle);
        if (playSound)
            audioManager.PlaySound(audioManager.audio_snapIdle);
        uiController.HideCorrector();
    }

    public void ShowHover()
    {        
        material.SetColor("_Color", Color.green);
        audioManager.PlaySound(audioManager.audio_snapCorrect);
        uiController.ShowCorrector();
    }

    public void Snap(FruitsController.FruitTypes _type)
    {
        IsSnapped = true;
        FruitSnapped = _type;
        meshRenderer.enabled = false;
        audioManager.PlaySound(audioManager.audio_snapComplete);
    }     
    
    public void UnSnap()
    {
        IsSnapped = false;
        FruitSnapped = FruitsController.FruitTypes.NONE;
        meshRenderer.enabled = true;
    }   
}
