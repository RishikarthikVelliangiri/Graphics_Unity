using UnityEngine;

public class BallPhysicsShooter : MonoBehaviour
{
    [SerializeField] private Transform[] goalTargets; // Assign random goal target positions in the Inspector
    [SerializeField] private Rigidbody rb;            // Ball's Rigidbody component
    [SerializeField] private float shotForce = 20f;   // Force multiplier for the ball
    [SerializeField] private GameObject kicker;       // Reference to the player model

    private Vector3 initialPosition; // Store initial ball position (if needed for reset)

    void Start()
    {
        initialPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        Debug.Log("BallPhysicsShooter initialized. Initial position: " + initialPosition);
    }

    // Called when another collider hits the ball.
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ball collided with object: " + collision.gameObject.name);

        // Check if the colliding object is our player model.
        if (collision.gameObject == kicker) // Fixed comparison issue
        {
            Debug.Log("Kicker collided with the ball.");
            ShootBall();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Ball exited");
    }

    // ShootBall picks a random target and applies velocity to the ball.
    void ShootBall()
    {   
        if (goalTargets.Length == 0)
        {
            Debug.LogError("No goal targets assigned!");
            return;
        }

        // Choose a random target.
        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];

        // Calculate direction: horizontal direction toward the target.
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep shot mostly horizontal.
        Vector3 shotDirection = directionToTarget.normalized;

        // Add a slight upward arc.
        shotDirection.y = 0.3f;

        Debug.Log("Ball shot towards: " + target.position + " with direction: " + shotDirection);

        // Apply velocity to the ball.
        rb.linearVelocity = shotDirection * shotForce; // Fixed incorrect property name (linearVelocity -> velocity)
    }

    // (Optional) Reset the ball to its initial position if needed.
    public void ResetBallPosition()
    {
        transform.position = initialPosition;
        rb.linearVelocity = Vector3.zero; // Fixed incorrect property name
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Ball reset to initial position.");
    }
}
