using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    private bool isRotate;
    private bool isClockwise;
    private float speed = 100f;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            StartRotation(true);
        else if (Input.GetKeyDown(KeyCode.W))
            StartRotation(false);
        else if (Input.GetKeyDown(KeyCode.E))
            StopRotation();

        if (!isRotate)
            return;


        if (isClockwise)
            transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);
        else
            transform.Rotate(-Vector3.up, Time.deltaTime * speed, Space.World);
    }

    public void StartRotation(bool clockwise)
    {
        isRotate = true;
        isClockwise = clockwise;
    }

    public void StopRotation()
    {
        isRotate = false;
    }
}
