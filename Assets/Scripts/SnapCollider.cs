using UnityEngine;
using System.Collections;

public class SnapCollider : MonoBehaviour
{
    public FruitsController.FruitTypes FruitType;
    //public int[] Order;
    public Vector3 PositionToSnap;
    public Quaternion RotiationToSnap;
    //public float Size;
    public Vector2 Size;

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
        material.SetFloat("_ScaleX", Size.x);
        material.SetFloat("_ScaleY", Size.y);
    }

    private void Hide(GameController.EndGameTypes _type)
    {
        meshRenderer.enabled = false;
    }

    public void ShowCorrect()
    {        
        material.SetColor("_Color", Color.green);
        audioManager.PlaySound(audioManager.audio_snapCorrect);
    }

    public void ShowIncorrect()
    {
        material.SetColor("_Color", Color.red);
        audioManager.PlaySound(audioManager.audio_snapIncorrect);
    }

    public void ShowIdle(bool playSound)
    {
        material.SetColor("_Color", colorIdle);
        if (playSound)
            audioManager.PlaySound(audioManager.audio_snapIdle);
    }

    public void Snap()
    {
        IsSnapped = true;
        meshRenderer.enabled = false;
        audioManager.PlaySound(audioManager.audio_snapComplete);
    }        
}
