using UnityEngine;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    private bool isRotate;
    private bool isClockwise;
    private const float SPEED = 100f;
    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.Instance;

#if UNITY_EDITOR
        var material = GetComponentInChildren<MeshRenderer>().material;
        material.CopyPropertiesFromMaterial(new Material(material));
        material.shader = Shader.Find("Mobile/Diffuse");
#endif
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
