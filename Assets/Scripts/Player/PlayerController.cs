using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravityForce;
    [SerializeField] private float camSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] Camera mainCam;
    Rigidbody rigidbody;

    float yRotation = 0;
    float xRotation = 0;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Gravity()
    {
        Vector3 centrePoint = new Vector3(Values.Instance.PlanetSize.x / 2, Values.Instance.PlanetSize.y / 2, Values.Instance.PlanetSize.z / 2);
        Vector3 g = (centrePoint - transform.position) * gravityForce; //Force of gravity pull
        rigidbody.velocity = g * Time.deltaTime;
        Vector3 localUp = rigidbody.rotation * Vector3.up; //Players initial up rotation
        Vector3 gravityUp = (transform.position - centrePoint).normalized; //Up vector from gravity's centre
        transform.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidbody.rotation;
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        rigidbody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void RotateCamera()
    {
        float camX = camSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
        float camY = camSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;

        yRotation += camX;
        xRotation -= camY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        mainCam.transform.rotation = Quaternion.Euler(transform.rotation.x + xRotation, yRotation, 0);
    }

    private void Update()
    {
        Gravity();
        RotateCamera();
        GetInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
}
