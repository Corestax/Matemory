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
        colorIdle = material.GetColor("_Color");
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
        
        // Resize ring to the smallest side       
        //material.SetFloat("_ScaleX", Size);
        //material.SetFloat("_ScaleY", Size);
    }

    private void Hide(GameController.EndGameTypes _type)
    {
        meshRenderer.enabled = false;
    }

    public void ShowIdle(bool playSound)
    {
        material.SetColor("_Color", colorIdle);
        if (playSound)
            audioManager.PlaySound(audioManager.audio_snapIdle);
    }

    public void ShowHover()
    {        
        material.SetColor("_Color", Color.green);
        audioManager.PlaySound(audioManager.audio_snapCorrect);
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
        //FruitSnapped = null;
        meshRenderer.enabled = true;
    }   
}
