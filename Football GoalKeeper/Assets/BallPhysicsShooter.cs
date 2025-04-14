using System.Collections;
using UnityEngine;
using TMPro;  // For TextMeshProUGUI
using UnityEngine.SceneManagement;  // For scene transitions

public class BallPhysicsShooter : MonoBehaviour
{
    [Header("Targets & Ball Settings")]
    [SerializeField] private Transform[] goalTargets;   // Random goal target positions (assign in Inspector)
    [SerializeField] private Rigidbody rb;              // Ball's Rigidbody component
    [SerializeField] private float shotForce = 20f;       // Force multiplier for the ball

    [Header("Player (Kicker) Settings")]
    [SerializeField] private GameObject kicker;           // Striker (player model) that kicks the ball
    [SerializeField] private Vector3 kickerResetPosition;   // Striker's idle/reset position (set in Inspector)
    [SerializeField] private Vector3 kickerResetRotation;   // Striker's idle/reset rotation in Euler angles

    [Header("Animation Settings")]
    [SerializeField] private Animator strikerAnimator;         // Animator for the striker (attached to kicker)
    [SerializeField] private AnimationClip kickerAnimationClip;  // Kicking animation clip (e.g., "3d Shot")

    [Header("VR Controllers")]
    [SerializeField] private GameObject[] controllers;    // VR controller objects (if needed)

    [Header("Goal Net")]
    [SerializeField] private GameObject goalNet;          // GoalNet object (assign via Inspector)

    [Header("Scoring & UI")]
    [SerializeField] private TextMeshProUGUI scoreText;     // Score display text (TextMeshProUGUI)
    private int score = 0;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip refereeWhistle;      // Referee whistle clip
    [SerializeField] private float refereeWhistleVolume = 1f; 
    [SerializeField] private AudioClip blockedSound;         // Sound for when the ball is blocked (stadium shouting)
    [SerializeField] private float blockedSoundVolume = 1f;
    [SerializeField] private AudioClip goalNetSound;         // Sound for when the ball hits the goal net (stadium "awww")
    [SerializeField] private float goalNetSoundVolume = 1f;
    [SerializeField] private AudioClip ambientSound;         // Ambient sound to play throughout the game
    [SerializeField] private float ambientVolume = 1f;

    [Header("Game Settings")]
    [SerializeField] private int totalShots = 10;           // Total shots per game

    [Header("Bounce Settings")]
    [SerializeField] private float bounceMultiplier = 1.5f; // Multiplier for extra bounce when blocked

    [Header("Optional Settings")]
    [SerializeField] private bool hideKickerUntilGameStart = true; // Hide striker until game begins

    // Internal state
    private Vector3 initialBallPosition;    // Starting ball position
    private bool isResetting = false;       // Prevent multiple resets in one cycle
    private bool hasScored = false;         // Award score only once per cycle
    private bool ballHitGoalNet = false;    // Flag if ball touched goal net before block
    private bool gameStarted = false;       // True when gameplay is active (after delay)
    private int currentShots;               // Shots remaining
    private bool firstShot = true;          // True for the very first shot

    // Ambient AudioSource for playing ambient sound continuously.
    private AudioSource ambientAudioSource;

    void Start()
    {
        // Store initial ball position.
        initialBallPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Initialize shot count.
        currentShots = totalShots;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        else
            Debug.LogWarning("[BallPhysicsShooter] Score Text not assigned!");

        // Set up ambient sound.
        if (ambientSound != null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.clip = ambientSound;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
            Debug.Log("[Audio] Ambient sound started.");
        }

        // Hide the striker until game starts.
        if (hideKickerUntilGameStart && kicker != null)
        {
            kicker.SetActive(false);
            Debug.Log("[Start] Kicker is hidden until game starts.");
        }

        // Force striker into Idle state at startup.
        if (strikerAnimator != null)
        {
            strikerAnimator.Rebind();
            strikerAnimator.Update(0f);
            strikerAnimator.Play("Idle", 0, 0f);  // Ensure "Idle" state exists.
            Debug.Log("[Start] Striker animator set to Idle.");
        }

        // Begin delayed start.
        StartCoroutine(DelayedStart());

        Debug.Log("[BallPhysicsShooter] Initialized. Ball pos: " + initialBallPosition);
        Debug.Log("[BallPhysicsShooter] Kicker reset pos: " + kickerResetPosition + ", rotation: " + kickerResetRotation);
    }

    // DelayedStart waits 4 seconds, plays the whistle, and then starts the first shot cycle.
    IEnumerator DelayedStart()
    {
        Debug.Log("[DelayedStart] Waiting 4 seconds before gameplay begins.");
        yield return new WaitForSeconds(4f);

        // Play whistle for first shot.
        if (refereeWhistle != null)
        {
            Debug.Log("[DelayedStart] Playing referee whistle for first shot: " + refereeWhistle.name);
            AudioSource.PlayClipAtPoint(refereeWhistle, Camera.main.transform.position, refereeWhistleVolume);
            yield return new WaitForSeconds(1f);
        }

        // Reactivate the striker if hidden.
        if (hideKickerUntilGameStart && kicker != null)
        {
            kicker.SetActive(true);
            Debug.Log("[DelayedStart] Kicker reactivated.");
        }

        // Force striker into Idle state.
        if (strikerAnimator != null)
        {
            strikerAnimator.Rebind();
            strikerAnimator.Update(0f);
            strikerAnimator.Play("Idle", 0, 0f);
            Debug.Log("[DelayedStart] Striker forced to Idle state.");
        }

        Debug.Log("[DelayedStart] 4 seconds elapsed. Starting first shot cycle.");
        gameStarted = true;
        firstShot = true;
        StartCoroutine(ResetPositionsAndAnimate());
    }

    void Update()
    {
        if (!gameStarted)
            return;

        // Optional: allow Enter key to trigger a reset cycle during gameplay.
        if (Input.GetKeyDown(KeyCode.Return) && !isResetting)
        {
            Debug.Log("[Update] Enter key pressed during gameplay. Initiating reset cycle.");
            StartCoroutine(ResetPositionsAndAnimate());
        }
    }

    // Coroutine to reset positions, play audio cues, reanimate striker, then shoot the ball.
    IEnumerator ResetPositionsAndAnimate()
    {
        isResetting = true;
        hasScored = false;
        ballHitGoalNet = false;

        if (!firstShot)
        {
            Debug.Log("[Reset] Waiting 5 seconds before next shot cycle.");
            yield return new WaitForSeconds(5f);

            // Depending on the result, play the appropriate sound.
            if (ballHitGoalNet)
            {
                if (goalNetSound != null)
                {
                    Debug.Log("[Reset] Playing goal net sound: " + goalNetSound.name);
                    AudioSource.PlayClipAtPoint(goalNetSound, Camera.main.transform.position, goalNetSoundVolume);
                }
            }
            else if (hasScored)
            {
                if (blockedSound != null)
                {
                    Debug.Log("[Reset] Playing blocked (celebration) sound: " + blockedSound.name);
                    AudioSource.PlayClipAtPoint(blockedSound, Camera.main.transform.position, blockedSoundVolume);
                }
            }
            else
            {
                // Play whistle for non-collision resets.
                if (refereeWhistle != null)
                {
                    Debug.Log("[Reset] Playing referee whistle: " + refereeWhistle.name);
                    AudioSource.PlayClipAtPoint(refereeWhistle, Camera.main.transform.position, refereeWhistleVolume);
                }
            }
            yield return new WaitForSeconds(1f);
        }
        else
        {
            firstShot = false;
        }

        // Reset ball and striker transforms.
        ResetBallPosition();
        ResetKickerTransform();

        // Rebind striker's animator.
        if (strikerAnimator != null)
        {
            strikerAnimator.Rebind();
            strikerAnimator.Update(0f);
            Debug.Log("[Reset] Striker animator rebound to Idle.");
        }
        yield return null;

        // For the last shot, skip striker's kick animation.
        if (currentShots > 1)
        {
            if (strikerAnimator != null && kickerAnimationClip != null)
            {
                strikerAnimator.Play(kickerAnimationClip.name, 0, 0f);
                Debug.Log("[Reset] Striker kick animation triggered: " + kickerAnimationClip.name);
            }
            else
            {
                Debug.LogWarning("[Reset] Missing striker animator or animation clip!");
            }
            yield return null;
            ShootBall();
        }
        else
        {
            Debug.Log("[Reset] Last shot - skipping striker animation.");
            ShootBall();
        }

        // Decrement shot count.
        currentShots--;
        Debug.Log("[Reset] Shot cycle complete. Shots remaining: " + currentShots);
        if (scoreText != null)
            scoreText.text = "Shots Remaining: " + currentShots;

        if (currentShots <= 0)
        {
            Debug.Log("[Game] All shots taken. Returning to menu (Scene 0).");
            SceneManager.LoadScene(0);
        }

        isResetting = false;
    }

    // Collision detection for scoring and bounce behavior.
    private void OnCollisionEnter(Collision collision)
    {
        if (!gameStarted)
            return;

        Debug.Log("[Collision] Ball collided with: " + collision.gameObject.name);

        // If ball hits the goal net, flag and reset without awarding score.
        if (collision.gameObject == goalNet)
        {
            ballHitGoalNet = true;
            Debug.Log("[Collision] Ball hit the goal net. No score awarded.");
            if (!isResetting)
                StartCoroutine(ResetPositionsAndAnimate());
            return;
        }
        // If ball collides with the striker, shoot the ball.
        else if (collision.gameObject == kicker)
        {
            Debug.Log("[Collision] Kicker collided with the ball. Shooting ball.");
            ShootBall();
            return;
        }
        // If ball collides with any VR controller, count as a block.
        else if (CheckControllersCollision(collision))
        {
            if (!hasScored && !ballHitGoalNet)
            {
                Debug.Log("[Collision] Ball blocked by VR controller. Awarding point.");
                AddScore();
                hasScored = true;
            }
            if (!isResetting)
                StartCoroutine(ResetPositionsAndAnimate());
            return;
        }
    }

    // Helper: Check if collision involves any VR controller.
    private bool CheckControllersCollision(Collision collision)
    {
        foreach (GameObject controller in controllers)
        {
            if (collision.gameObject == controller)
                return true;
        }
        return false;
    }

    // Increment score and update the UI.
    private void AddScore()
    {
        score++;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        Debug.Log("[Score] Score increased to: " + score);
    }

    // Bounce the ball using the collision's contact normal with extra bounce.
    void BounceBall(Collision collision)
    {
        Vector3 bounceDirection = collision.contacts[0].normal;
        bounceDirection.y = Mathf.Abs(bounceDirection.y);
        rb.linearVelocity = bounceDirection.normalized * shotForce * bounceMultiplier;
        Debug.Log("[Bounce] Ball bounced with direction: " + bounceDirection + " (Multiplier: " + bounceMultiplier + ")");
    }

    // Shoot the ball toward a random goal target.
    void ShootBall()
    {
        if (goalTargets.Length == 0)
        {
            Debug.LogError("[Shoot] No goal targets assigned!");
            return;
        }
        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0;
        Vector3 shotDirection = directionToTarget.normalized;
        shotDirection.y = 0.3f;
        Debug.Log("[Shoot] Ball shot toward: " + target.position + " with direction: " + shotDirection);
        rb.linearVelocity = shotDirection * shotForce;
    }

    // Reset the ball's position and stop all movement.
    public void ResetBallPosition()
    {
        transform.position = initialBallPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("[Reset] Ball reset to initial position: " + initialBallPosition);
    }

    // Reset the striker's transform based on manually assigned values.
    public void ResetKickerTransform()
    {
        if (kicker != null)
        {
            kicker.transform.position = kickerResetPosition;
            kicker.transform.rotation = Quaternion.Euler(kickerResetRotation);
            Debug.Log("[Reset] Kicker reset to position: " + kickerResetPosition + ", rotation: " + kickerResetRotation);
        }
    }
}
