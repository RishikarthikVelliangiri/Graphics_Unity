using UnityEngine;

public class BallPhysicsShooter : MonoBehaviour
{
    public Transform[] goalTargets; // Assign goal targets in Inspector
    public Rigidbody rb;
    public float shotForce = 20f;
    public float arcHeight = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Ensure Rigidbody is attached
    }

    [ContextMenu("Shoot Ball")] // Adds a button in Inspector
    public void ShootBall()
    {
        if (goalTargets.Length == 0)
        {
            Debug.LogWarning("No goal targets assigned!");
            return;
        }

        // Select a random goal target
        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];

        // Calculate the trajectory to hit the target
        Vector3 shotDirection = CalculateTrajectory(transform.position, target.position, arcHeight);

        // Reset velocity to ensure consistent shots
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply force to shoot the ball
        rb.AddForce(shotDirection, ForceMode.Impulse);

        Debug.Log($"Ball shot towards: {target.name}");
    }

    private Vector3 CalculateTrajectory(Vector3 start, Vector3 end, float height)
    {
        Vector3 direction = end - start;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        float distance = flatDirection.magnitude;
        float yOffset = direction.y;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float speedY = Mathf.Sqrt(2 * gravity * height);
        float time = (speedY + Mathf.Sqrt(speedY * speedY + 2 * gravity * yOffset)) / gravity;
        float speedXZ = distance / time;

        Vector3 result = flatDirection.normalized * speedXZ;
        result.y = speedY;

        return result;
    }
}
