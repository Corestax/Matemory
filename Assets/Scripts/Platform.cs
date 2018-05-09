using UnityEngine;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    private bool isRotate;
    private bool isClockwise;
    private const float SPEED = 100f;
    private AudioManager audioManager;

    public Material Material { get; private set; }

    void Start()
    {
        audioManager = AudioManager.Instance;

        Material = GetComponentInChildren<MeshRenderer>().material;
        Material.CopyPropertiesFromMaterial(new Material(Material));
    }

    void Update()
    {
        if (!isRotate)
            return;

        if (isClockwise)
            transform.Rotate(Vector3.up * Time.deltaTime * SPEED, Space.World);
        else
            transform.Rotate(-Vector3.up, Time.deltaTime * SPEED, Space.World);
    }

    public void Rotate(bool clockwise)
    {
        audioManager.PlaySpinSound();
        isRotate = true;
        isClockwise = clockwise;
    }

    public void StopRotate()
    {
        audioManager.StopSpinSound();
        isRotate = false;
    }
}
