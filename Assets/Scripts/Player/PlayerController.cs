using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravityForce;
    [SerializeField] private ForceMode forceMode;

    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Gravity()
    {
        Vector3 centrePoint = new Vector3(Values.Instance.PlanetSize.x / 2, Values.Instance.PlanetSize.y / 2, Values.Instance.PlanetSize.z / 2);
        Vector3 g = (centrePoint - transform.position) * gravityForce;
        rigidbody.velocity = g * Time.deltaTime;
        Vector3 localUp = rigidbody.rotation * Vector3.up;
        Vector3 gravityUp = (transform.position - centrePoint).normalized;
        transform.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidbody.rotation;
    }

    private void Update()
    {
        Gravity();
    }

}
