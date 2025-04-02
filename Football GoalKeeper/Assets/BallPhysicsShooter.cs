using UnityEngine;

public class BallPhysicsShooter : MonoBehaviour
{
    [SerializeField] private Transform[] goalTargets; // Random goal target positions assigned in the Inspector
    [SerializeField] private Rigidbody rb;            // Ball's Rigidbody component
    [SerializeField] private float shotForce = 20f;     // Force multiplier for the ball
    [SerializeField] private GameObject kicker;         // Reference to the player model (kicker)
    [SerializeField] private GameObject goalkeeper;     // Reference to the goalkeeper

    private Vector3 initialBallPosition;     // Initial ball position
    private Vector3 initialKickerPosition;   // Initial kicker (player) position

    void Start()
    {
        initialBallPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (kicker != null)
            initialKickerPosition = kicker.transform.position;

        Debug.Log("BallPhysicsShooter initialized. Ball initial position: " + initialBallPosition);
        Debug.Log("Kicker initial position: " + initialKickerPosition);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Reset positions for both the ball and kicker.
            ResetBallPosition();
            ResetKickerPosition();

            // Optional: Trigger the kick animation if the kicker has an Animator.
            // Animator anim = kicker.GetComponent<Animator>();
            // if (anim != null)
            //     anim.SetTrigger("Kick");

            // Shoot the ball after resetting.
            ShootBall();
        }
    }

    // Called when another collider hits the ball.
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ball collided with object: " + collision.gameObject.name);

        // If the ball collides with the kicker, shoot the ball.
        if (collision.gameObject == kicker)
        {
            Debug.Log("Kicker collided with the ball.");
            ShootBall();
        }
        // If the ball collides with the goalkeeper, bounce it back.
        else if (collision.gameObject == goalkeeper)
        {
            Debug.Log("Goalkeeper collided with the ball. Bouncing back.");
            BounceBall(collision);
        }
    }

    // BounceBall calculates a reflection off the collision normal and applies that velocity.
    void BounceBall(Collision collision)
    {
        // Get the first contact point and use its normal for reflection.
        Vector3 bounceDirection = collision.contacts[0].normal;
        // Ensure an upward component.
        bounceDirection.y = Mathf.Abs(bounceDirection.y);
        rb.linearVelocity = bounceDirection.normalized * shotForce;
        Debug.Log("Ball bounced with direction: " + bounceDirection);
    }

    // ShootBall chooses a random target and applies velocity to the ball.
    void ShootBall()
    {   
        if (goalTargets.Length == 0)
        {
            Debug.LogError("No goal targets assigned!");
            return;
        }

        // Choose a random target.
        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];

        // Calculate horizontal direction toward the target.
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep shot mostly horizontal.
        Vector3 shotDirection = directionToTarget.normalized;
        
        // Add a slight upward arc.
        shotDirection.y = 0.3f;

        Debug.Log("Ball shot towards: " + target.position + " with direction: " + shotDirection);

        // Apply velocity to the ball.
        rb.linearVelocity = shotDirection * shotForce;
    }

    // ResetBallPosition resets the ball to its original position and stops its motion.
    public void ResetBallPosition()
    {
        transform.position = initialBallPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Ball reset to initial position: " + initialBallPosition);
    }

    // ResetKickerPosition resets the kicker (player) to its original position.
    public void ResetKickerPosition()
    {
        if (kicker != null)
        {
            kicker.transform.position = initialKickerPosition;
            Debug.Log("Kicker reset to initial position: " + initialKickerPosition);
        }
    }
}
