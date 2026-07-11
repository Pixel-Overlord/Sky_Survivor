using UnityEngine;

/// <summary>
/// Moves the plane through the city. W accelerates it along its current heading,
/// while A and D steer that heading left or right without allowing a backwards turn.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField, Min(1f)] private float forwardSpeed = 60f;
    [SerializeField, Min(1f)] private float turnSpeedDegrees = 90f;
    [SerializeField, Range(1f, 90f)] private float maximumTurnAngle = 90f;

    private Rigidbody planeBody;
    private Quaternion startingHeading;
    private float headingOffset;
    private float forwardInput;
    private float turnInput;

    /// <summary>Provides the plane physics body to systems such as scoring and camera follow.</summary>
    public Rigidbody PlaneBody => planeBody;

    private void Awake()
    {
        planeBody = GetComponent<Rigidbody>();
        startingHeading = transform.rotation;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        ReadFlightInput();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        Quaternion targetHeading = UpdateHeading();
        ApplyForwardVelocity(targetHeading);
    }

    /// <summary>
    /// Reads only forward throttle and yaw. Negative Vertical input is ignored so the plane cannot reverse.
    /// </summary>
    private void ReadFlightInput()
    {
        forwardInput = Mathf.Max(0f, Input.GetAxisRaw("Vertical"));
        turnInput = Input.GetAxisRaw("Horizontal");
    }

    /// <summary>
    /// Turns relative to the orientation at the start of the run. Clamping the offset to +/- 90 degrees
    /// lets the player fly left or right, but never turn around and leave the generated city behind.
    /// </summary>
    private Quaternion UpdateHeading()
    {
        headingOffset += turnInput * turnSpeedDegrees * Time.fixedDeltaTime;
        headingOffset = Mathf.Clamp(headingOffset, -maximumTurnAngle, maximumTurnAngle);

        Quaternion targetHeading = startingHeading * Quaternion.Euler(0f, headingOffset, 0f);
        planeBody.MoveRotation(targetHeading);
        return targetHeading;
    }

    /// <summary>Applies velocity along the yaw-adjusted heading while preserving the plane's current altitude.</summary>
    private void ApplyForwardVelocity(Quaternion heading)
    {
        Vector3 travelDirection = heading * Vector3.forward;
        Vector3 velocity = travelDirection * (forwardSpeed * forwardInput);
        velocity.y = 0f;
        planeBody.velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
            GameManager.Instance.GameOver();
    }
}
