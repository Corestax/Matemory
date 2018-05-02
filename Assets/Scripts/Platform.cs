using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            Rotate(true);
        else if (Input.GetKeyDown(KeyCode.W))
            Rotate(false);
        else if (Input.GetKeyDown(KeyCode.E))
            StopRotate();

        if (!isRotate)
            return;


        if (isClockwise)
            transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);
        else
            transform.Rotate(-Vector3.up, Time.deltaTime * speed, Space.World);
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
