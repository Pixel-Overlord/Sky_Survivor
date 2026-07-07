using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float movementSpeed = 10f;

    [Header("Rotation")]
    [SerializeField] private float pitchSpeed = 60f;
    [SerializeField] private float rollAngle = 30f;
    [SerializeField] private float rollSpeed = 5f;

    private Rigidbody rb;

    private float verticalInput;
    private float horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        move();
    }

    private void move()
    {
        Vector3 movement = new Vector3(
        horizontalInput * movementSpeed,
        verticalInput * movementSpeed,
        forwardSpeed);

        rb.velocity = movement;
    }
}