using UnityEngine;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    private bool isRotate;
    private bool isClockwise;
    private const float SPEED = 100f;

    void Start()
    {

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
        isRotate = true;
        isClockwise = clockwise;
    }

    public void StopRotate()
    {
        isRotate = false;
    }
}
