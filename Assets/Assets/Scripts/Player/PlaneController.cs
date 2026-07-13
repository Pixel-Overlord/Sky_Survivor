using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float movementSpeed = 10f;

    [SerializeField] private Transform ground;

    private Rigidbody rb;

    public Rigidbody planeGetter => rb;

    private float verticalInput;
    private float horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        move();
    }

    private void move()
    {
        if (horizontalInput < 0)    // Player has pressed A
        {
            rb.GetComponent<Animator>().Play("Tilt Left");
        }
        else if (horizontalInput > 0)
        {
            rb.GetComponent<Animator>().Play("Tilt Right");
        }

        Vector3 movement = new Vector3(
        horizontalInput * movementSpeed,
        verticalInput * movementSpeed,
        forwardSpeed);

        rb.velocity = movement;
    }   

    /*
     * If there is any collision between player and the Obstacle, Game Manager sets the Game as over and log a message.
     */
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}