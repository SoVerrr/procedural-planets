using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravityForce;
    [SerializeField] private float camSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Camera cam;
    Rigidbody rigidbody;



    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
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
        moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        rigidbody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private ChunkData CastEditRay(ref Vector3Int hitPosition) //Returns chunkID if it hits a chunk or a Vector3Int of -1 -1 -1 if it doesnt
    {
        RaycastHit hit = new RaycastHit();

        if(Physics.Raycast(transform.position, transform.forward, out hit))
        {
            ChunkData hitTest = hit.transform.GetComponent<ChunkData>();
            
            hitPosition = Vector3Int.RoundToInt(hit.point);
            

            if (hitTest != null)
                return hit.transform.GetComponent<ChunkData>();
        }

        return null;
    }

    private void Update()
    {
        //Gravity();
        GetInput();
        transform.rotation = cam.transform.rotation;

        if (Input.GetMouseButtonDown(0))
        {

            Vector3Int hitPosition = new Vector3Int();
            ChunkData chunk = CastEditRay(ref hitPosition);

            PlanetMap.planetMap[hitPosition.x, hitPosition.y, hitPosition.z] -= 10;
            Debug.Log($"HitPos: {hitPosition} | Value after: ");
            chunk.EditChunk();
        }
        if (Input.GetMouseButtonDown(1))
        {

            Vector3Int hitPosition = new Vector3Int();
            ChunkData chunk = CastEditRay(ref hitPosition);

            PlanetMap.planetMap[hitPosition.x, hitPosition.y, hitPosition.z] += 10;
            Debug.Log($"HitPos: {hitPosition} | Value after: ");
            chunk.EditChunk();
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
}
