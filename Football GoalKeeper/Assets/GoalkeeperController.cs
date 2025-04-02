using UnityEngine;

public class GoalkeeperController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Movement speed

    void Update()
    {
        float horizontal = 0f;
        
        // If holding A, move left; if holding D, move right.
        if (Input.GetKey(KeyCode.A))
        {
            horizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontal = 1f;
        }
        
        // Translate the goalkeeper along the x-axis
        transform.Translate(Vector3.right * horizontal * moveSpeed * Time.deltaTime);
    }
}
