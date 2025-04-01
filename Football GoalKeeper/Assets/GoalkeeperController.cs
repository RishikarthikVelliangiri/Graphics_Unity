using UnityEngine;
using UnityEngine.InputSystem;

public class GoalkeeperController : MonoBehaviour
{
    public float moveSpeed = 5f;       // Horizontal movement speed
    public float bounceForce = 15f;    // Force applied to the ball when hit
    private Vector2 moveInput;         // Stores left/right input

    // Called by the new Input System when A/D or left/right keys are pressed
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Move goalkeeper along the x-axis
        float moveAmount = moveInput.x * moveSpeed * Time.fixedDeltaTime;
        transform.Translate(new Vector3(moveAmount, 0, 0));
    }

    // When the goalkeeper collides with the ball, bounce it out.
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRb = collision.gameObject.GetComponent<Rigidbody>();
            if(ballRb != null)
            {
                // Use collision contact normal to determine bounce direction.
                Vector3 bounceDirection = collision.contacts[0].normal;
                // Ensure a slight upward arc.
                bounceDirection.y = Mathf.Clamp(bounceDirection.y, 0.3f, 1f);
                ballRb.linearVelocity = bounceDirection.normalized * bounceForce;
                Debug.Log("Ball bounced off goalkeeper");
            }
        }
    }
}
