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
    [SerializeField] private Vector3 kickerResetPosition; // Striker's idle/reset position (set in Inspector)
    [SerializeField] private Vector3 kickerResetRotation; // Striker's idle/reset rotation (Euler angles)

    [Header("Animation Settings")]
    [SerializeField] private Animator strikerAnimator;         // Animator for the striker (attached to kicker)
    [SerializeField] private AnimationClip kickerAnimationClip;  // Kicking animation clip (e.g., "3d Shot")

    [Header("VR Controllers")]
    [SerializeField] private GameObject[] controllers;    // VR controller objects (if needed)

    [Header("Goalkeeper Hands")]
    [SerializeField] private GameObject[] goalkeeperHands;    // Goalkeeper hand objects (drag into Inspector)
    [SerializeField] private float handsBounceMultiplier = 0.8f;  // Bounce multiplier for goalkeeper hand collisions

    [Header("Goal Net")]
    [SerializeField] private GameObject goalNet;          // GoalNet object (assign via Inspector)

    [Header("Goal Scoring Colliders")]
    // Note: Instead of a separate field, the goal colliders should be children of the goalNet.
    // The code will detect a collision if the collided object is goalNet or is a child of goalNet.
    
    [Header("Ground Settings")]
    [SerializeField] private GameObject groundObject;     // Ground object (assign via Inspector)
    [SerializeField] private float groundBounceMultiplier = 0.6f;  // Multiplier for bounce when ball hits the ground
    [SerializeField] private float groundBounceThreshold = 0.5f;   // Threshold below which the ball stops bouncing

    [Header("Scoring & UI")]
    [SerializeField] private TextMeshProUGUI scoreText;     // Score display text (TextMeshProUGUI)
    private int score = 0;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip refereeWhistle;      // Referee whistle clip
    [SerializeField] private float refereeWhistleVolume = 1f;
    [SerializeField] private AudioClip blockedSound;         // Sound when ball is blocked (celebration)
    [SerializeField] private float blockedSoundVolume = 1f;
    [SerializeField] private AudioClip goalNetSound;         // Sound when ball hits the goal net (boo sound)
    [SerializeField] private float goalNetSoundVolume = 1f;
    [SerializeField] private AudioClip ambientSound;         // Ambient sound to play throughout the game
    [SerializeField] private float ambientVolume = 1f;

    [Header("Game Settings")]
    [SerializeField] private int totalShots = 10;           // Total shots per game

    [Header("Bounce Settings")]
    [SerializeField] private float bounceMultiplier = 1.5f; // Multiplier for bounce when blocked by VR controllers

    [Header("Optional Settings")]
    [SerializeField] private bool hideKickerUntilGameStart = true;  // Hide striker until game starts

    [Header("Feature Toggles")]
    [SerializeField] private bool enableCustomGroundBounce = true;   // Use custom bounce for ground collisions
    [SerializeField] private bool enableCustomHandBounce = true;     // Use custom bounce for goalkeeper hand collisions
    [SerializeField] private bool enableCustomBounceBehavior = true; // Use custom bounce behavior overall

    // Internal state
    private Vector3 initialBallPosition;    // Starting ball position
    private bool isResetting = false;       // Prevent multiple resets per cycle
    private bool hasScored = false;         // Ensure score is awarded only once per cycle
    private bool ballHitGoalNet = false;    // True if ball touched goal net before block
    private bool gameStarted = false;       // True when gameplay is active (after delay)
    private int currentShots;               // Shots remaining
    private bool firstShot = true;          // True for the very first shot
    private int shotsFired = 0;             // Shots fired (for UI display)

    // Ambient AudioSource for looping ambient sound.
    private AudioSource ambientAudioSource;

    void Start()
    {
        // Store the initial ball position.
        initialBallPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Initialize shot count.
        currentShots = totalShots;
        shotsFired = 0;
        UpdateScoreUI();

        // Set up ambient sound on loop.
        if (ambientSound != null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.clip = ambientSound;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
            Debug.Log("[Audio] Ambient sound started on loop.");
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

        // Begin the delayed start.
        StartCoroutine(DelayedStart());

        Debug.Log("[BallPhysicsShooter] Initialized. Ball pos: " + initialBallPosition);
        Debug.Log("[BallPhysicsShooter] Kicker reset pos: " + kickerResetPosition + ", rotation: " + kickerResetRotation);
    }

    // DelayedStart waits 4 seconds before starting gameplay.
    IEnumerator DelayedStart()
    {
        Debug.Log("[DelayedStart] Waiting 4 seconds before gameplay begins.");
        yield return new WaitForSeconds(4f);

        // Play referee whistle for first shot.
        if (refereeWhistle != null)
        {
            Debug.Log("[DelayedStart] Playing referee whistle for first shot: " + refereeWhistle.name);
            AudioSource.PlayClipAtPoint(refereeWhistle, Camera.main.transform.position, refereeWhistleVolume);
            yield return new WaitForSeconds(1f);
        }

        // Activate the striker.
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

        // Optional: allow Enter key to trigger a reset cycle manually during gameplay.
        if (Input.GetKeyDown(KeyCode.Return) && !isResetting)
        {
            Debug.Log("[Update] Enter key pressed during gameplay. Initiating reset cycle.");
            StartCoroutine(ResetPositionsAndAnimate());
        }
    }

    // Coroutine to reset positions, play audio cues, trigger striker animation (if applicable), then shoot the ball.
    IEnumerator ResetPositionsAndAnimate()
    {
        isResetting = true;
        hasScored = false;
        ballHitGoalNet = false;

        // For subsequent shot cycles (not the first), wait 5 seconds.
        if (!firstShot)
        {
            Debug.Log("[Reset] Waiting 5 seconds before next shot cycle.");
            yield return new WaitForSeconds(5f);

            if (refereeWhistle != null)
            {
                Debug.Log("[Reset] Playing referee whistle: " + refereeWhistle.name);
                AudioSource.PlayClipAtPoint(refereeWhistle, Camera.main.transform.position, refereeWhistleVolume);
            }
            yield return new WaitForSeconds(1f);
        }
        else
        {
            firstShot = false;
            // For the first shot, the whistle was already played in DelayedStart.
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

        // For all shots except the last, trigger striker's kick animation; skip for last shot.
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

        currentShots--;
        Debug.Log("[Reset] Shot cycle complete. Shots remaining: " + currentShots);
        UpdateScoreUI();

        if (currentShots <= 0)
        {
            Debug.Log("[Game] All shots taken. Waiting 5 seconds before returning to menu (Scene 0).");
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene(0);
        }
        isResetting = false;
    }

    // Collision detection for scoring, bouncing, etc.
    private void OnCollisionEnter(Collision collision)
    {
        if (!gameStarted)
            return;

        // Prevent processing collision if already resetting.
        if (isResetting) return;

        Debug.Log("[Collision] Ball collided with: " + collision.gameObject.name);

        // If ball collides with the ground, perform realistic ground bounce.
        if (groundObject != null && collision.gameObject == groundObject)
        {
            Debug.Log("[Collision] Ball hit the ground.");
            if (enableCustomBounceBehavior && enableGroundBounce())
                BounceBallGround(collision);
            return;  // Do not trigger reset cycle on ground bounce.
        }
        // If ball hits the goal net (or any child of goalNet), treat it as a goal (no point awarded).
        else if (goalNet != null && (collision.gameObject == goalNet || collision.transform.IsChildOf(goalNet.transform)))
        {
            ballHitGoalNet = true;
            Debug.Log("[Collision] Ball hit the goal net (or its colliders). No score awarded.");
            if (goalNetSound != null)
            {
                Debug.Log("[Collision] Playing goal net sound: " + goalNetSound.name);
                AudioSource.PlayClipAtPoint(goalNetSound, Camera.main.transform.position, goalNetSoundVolume);
            }
            if (!isResetting)
                StartCoroutine(ResetPositionsAndAnimate());
            return;
        }
        // Ignore collisions with the striker.
        else if (collision.gameObject == kicker)
        {
            Debug.Log("[Collision] Ignoring collision with striker.");
            return;
        }
        // If ball collides with any VR controller, treat it as blocked.
        else if (CheckControllersCollision(collision))
        {
            if (hasScored || ballHitGoalNet)
                return;  // Prevent awarding points twice
            Debug.Log("[Collision] Ball blocked by VR controller. Awarding point.");
            hasScored = true;  // Set the flag immediately to prevent double scoring.
            AddScore();
            if (blockedSound != null)
            {
                Debug.Log("[Collision] Playing blocked sound: " + blockedSound.name);
                AudioSource.PlayClipAtPoint(blockedSound, Camera.main.transform.position, blockedSoundVolume);
            }
            if (!isResetting)
                StartCoroutine(ResetPositionsAndAnimate());
            return;
        }
        // If ball collides with any goalkeeper hand, use custom hand bounce.
        else if (CheckGoalkeeperHandsCollision(collision))
        {
            Debug.Log("[Collision] Ball touched goalkeeper's hand. Bouncing with hand multiplier.");
            if (enableCustomBounceBehavior && enableGoalkeeperHandBounce())
                BounceBallWithMultiplier(collision, handsBounceMultiplier);
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

    // Helper: Check if collision involves any goalkeeper hand.
    private bool CheckGoalkeeperHandsCollision(Collision collision)
    {
        if (goalkeeperHands != null)
        {
            foreach (GameObject hand in goalkeeperHands)
            {
                if (collision.gameObject == hand)
                    return true;
            }
        }
        return false;
    }

    // Increment score and update UI.
    private void AddScore()
    {
        score++;
        UpdateScoreUI();
        Debug.Log("[Score] Score increased to: " + score);
    }

    // Update the score UI in the format "Score: X | Shots: Y/Total"
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score + " | Shots: " + (totalShots - currentShots) + "/" + totalShots;
        else
            Debug.LogWarning("[BallPhysicsShooter] Score Text not assigned!");
    }

    // BounceBallGround: Reflect ball velocity on ground collision using groundBounceMultiplier and realistic energy loss.
    void BounceBallGround(Collision collision)
    {
        Vector3 newVelocity = Vector3.Reflect(rb.linearVelocity, collision.contacts[0].normal) * groundBounceMultiplier;
        if (newVelocity.magnitude < groundBounceThreshold)
            newVelocity = Vector3.zero;
        rb.linearVelocity = newVelocity;
        Debug.Log("[BounceGround] Ball bounced off ground with new velocity: " + newVelocity);
    }

    // BounceBallWithMultiplier: Reflect ball velocity with given multiplier (for goalkeeper hands).
    void BounceBallWithMultiplier(Collision collision, float multiplier)
    {
        Vector3 newVelocity = Vector3.Reflect(rb.linearVelocity, collision.contacts[0].normal) * multiplier;
        rb.linearVelocity = newVelocity;
        Debug.Log("[BounceHand] Ball bounced with new velocity: " + newVelocity + " (Multiplier: " + multiplier + ")");
    }

    // BounceBall: Custom bounce for VR controller blocks using bounceMultiplier.
    void BounceBall(Collision collision)
    {
        Vector3 bounceDirection = collision.contacts[0].normal;
        bounceDirection.y = Mathf.Abs(bounceDirection.y);
        rb.linearVelocity = bounceDirection.normalized * shotForce * bounceMultiplier;
        Debug.Log("[Bounce] Ball bounced with direction: " + bounceDirection + " (Multiplier: " + bounceMultiplier + ")");
    }

    // ShootBall: Shoot the ball toward a random goal target.
    void ShootBall()
    {
        if (goalTargets.Length == 0)
        {
            Debug.LogError("[Shoot] No goal targets assigned!");
            return;
        }
        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0;  // Keep shot horizontal.
        Vector3 shotDirection = directionToTarget.normalized;
        shotDirection.y = 0.3f;   // Slight upward arc.
        Debug.Log("[Shoot] Ball shot toward: " + target.position + " with direction: " + shotDirection);
        rb.linearVelocity = shotDirection * shotForce;
    }

    // ResetBallPosition: Move the ball back to its initial position and zero its velocity.
    public void ResetBallPosition()
    {
        transform.position = initialBallPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("[Reset] Ball reset to initial position: " + initialBallPosition);
    }

    // ResetKickerTransform: Reset the striker's transform using the specified values.
    public void ResetKickerTransform()
    {
        if (kicker != null)
        {
            kicker.transform.position = kickerResetPosition;
            kicker.transform.rotation = Quaternion.Euler(kickerResetRotation);
            Debug.Log("[Reset] Kicker reset to position: " + kickerResetPosition + ", rotation: " + kickerResetRotation);
        }
    }

    // Feature toggle helper for ground bounce.
    private bool enableGroundBounce()
    {
        return groundObject != null;
    }

    // Feature toggle helper for goalkeeper hand bounce.
    private bool enableGoalkeeperHandBounce()
    {
        return (goalkeeperHands != null && goalkeeperHands.Length > 0);
    }
}
