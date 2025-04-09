using UnityEngine;

public class BallPhysicsShooter : MonoBehaviour
{
    [SerializeField] private Transform[] goalTargets;  // Random goal target positions assigned in the Inspector
    [SerializeField] private Rigidbody rb;             // Ball's Rigidbody component
    [SerializeField] private float shotForce = 20f;      // Force multiplier for the ball
    [SerializeField] private GameObject kicker;          // Reference to the striker (player model)
    [SerializeField] private GameObject goalkeeper;      // Reference to the goalkeeper
    [SerializeField] private GameObject[] controllers;   // VR controller objects assigned in the Inspector

    // Animator for the striker's reanimation and trigger parameter for the kick animation
    [SerializeField] private Animator strikerAnimator;
    [SerializeField] private string strikerResetTrigger = "ReKick";

    private Vector3 initialBallPosition;   // Initial ball position
    private Vector3 initialKickerPosition; // Initial striker (kicker) position

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
        // Revert to using the Enter key to trigger the reset and shot
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ResetBallPosition();
            ResetKickerPosition();

            // Trigger striker reanimation (kicking animation)
            if (strikerAnimator != null && !string.IsNullOrEmpty(strikerResetTrigger))
            {
                strikerAnimator.SetTrigger(strikerResetTrigger);
            }

            ShootBall();
        }
    }

    // Called when another collider hits the ball.
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ball collided with object: " + collision.gameObject.name);

        // If the ball collides with the striker (kicker), trigger a shot.
        if (collision.gameObject == kicker)
        {
            Debug.Log("Kicker collided with the ball.");
            ShootBall();
        }
        // If it collides with the goalkeeper, bounce it back.
        else if (collision.gameObject == goalkeeper)
        {
            Debug.Log("Goalkeeper collided with the ball. Bouncing back.");
            BounceBall(collision);
        }
        // Check if the collided object is one of the assigned VR controllers.
        else
        {
            foreach (GameObject controller in controllers)
            {
                if (collision.gameObject == controller)
                {
                    Debug.Log("VR controller collided with the ball. Bouncing off.");
                    BounceBall(collision);
                    break;
                }
            }
        }
    }

    // BounceBall calculates a reflection based on the collision normal and applies that velocity.
    void BounceBall(Collision collision)
    {
        // Get the first contact point's normal for reflection.
        Vector3 bounceDirection = collision.contacts[0].normal;
        // Ensure an upward component.
        bounceDirection.y = Mathf.Abs(bounceDirection.y);
        rb.linearVelocity = bounceDirection.normalized * shotForce;
        Debug.Log("Ball bounced with direction: " + bounceDirection);
    }

    // ShootBall selects a random target and applies velocity to the ball.
    void ShootBall()
    {
        if (goalTargets.Length == 0)
        {
            Debug.LogError("No goal targets assigned!");
            return;
        }

        // Choose a random target.
        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];
        // Calculate the horizontal direction toward the target.
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Ensure shot is mostly horizontal.
        Vector3 shotDirection = directionToTarget.normalized;
        
        // Add a slight upward arc.
        shotDirection.y = 0.3f;

        Debug.Log("Ball shot towards: " + target.position + " with direction: " + shotDirection);
        rb.linearVelocity = shotDirection * shotForce;
    }

    // ResetBallPosition resets the ball to its initial position and stops its motion.
    public void ResetBallPosition()
    {
        transform.position = initialBallPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Ball reset to initial position: " + initialBallPosition);
    }

    // ResetKickerPosition resets the striker (kicker) to its original position.
    public void ResetKickerPosition()
    {
        if (kicker != null)
        {
            kicker.transform.position = initialKickerPosition;
            Debug.Log("Kicker reset to initial position: " + initialKickerPosition);
        }
    }
}
