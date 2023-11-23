using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject player;

    
    float inputX, inputY;
    [SerializeField] float sensX, sensY;
    float rotationX, rotationY;

    private Vector3 centrePoint;
    void Start()
    {

    }

    void Update()
    {
        inputX = Input.GetAxis("Mouse X");
        inputY = Input.GetAxis("Mouse Y");

        rotationX -= inputY * Time.deltaTime * sensX;
        rotationY += inputX * Time.deltaTime * sensX;

        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.eulerAngles = new Vector3(rotationX, rotationY, 0);
    }
}
