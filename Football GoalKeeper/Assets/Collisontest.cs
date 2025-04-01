using UnityEngine;

public class BallCollisionTest : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ball hit: " + collision.gameObject.name);

    }
}
