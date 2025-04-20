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
    [SerializeField] private float delayBetweenShots = 5.0f; // *** NEW: Delay in seconds between shots ***

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
    private bool hasScoredThisShot = false; // *** CHANGED: Renamed from hasScored, ensures score awarded only once per shot cycle ***
    private bool ballHitGoalNet = false;    // True if ball touched goal net directly (not after a hand hit)
    private bool ballTouchedHandThisShot = false; // *** NEW: Tracks if a hand was hit during the current shot ***
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
        shotsFired = 0; // shotsFired will increment as shots are taken
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
            yield return new WaitForSeconds(1f); // Wait for whistle to finish
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

        Debug.Log("[DelayedStart] Starting first shot cycle.");
        gameStarted = true;
        firstShot = true;
        StartCoroutine(ResetPositionsAndAnimate());
    }

    void Update()
    {
        if (!gameStarted)
            return;

        // Optional: allow Enter key to trigger a reset cycle manually during gameplay.
        if (Input.GetKeyDown(KeyCode.Return) && !isResetting && currentShots > 0) // Don't allow manual reset if game over
        {
            Debug.Log("[Update] Enter key pressed during gameplay. Initiating reset cycle.");
            // Stop existing reset coroutine if any (to prevent overlap)
            StopCoroutine("ResetPositionsAndAnimate");
            StartCoroutine(ResetPositionsAndAnimate());
        }
    }

    // Coroutine to reset positions, play audio cues, trigger striker animation (if applicable), then shoot the ball.
    IEnumerator ResetPositionsAndAnimate()
    {
        // Prevent starting a new reset if one is already in progress
        if (isResetting)
        {
            Debug.LogWarning("[Reset] Already resetting, exiting new Reset coroutine call.");
            yield break;
        }

        isResetting = true;
        hasScoredThisShot = false; // Reset score flag for the new shot
        ballHitGoalNet = false;    // Reset goal net flag
        ballTouchedHandThisShot = false; // *** NEW: Reset hand touch flag ***

        // For subsequent shot cycles (not the first), wait for the specified delay.
        if (!firstShot)
        {
            Debug.Log($"[Reset] Waiting {delayBetweenShots} seconds before next shot cycle."); // *** CHANGED: Use variable delay ***
            yield return new WaitForSeconds(delayBetweenShots); // *** CHANGED: Use variable delay ***

            // Check if game ended while waiting (e.g., manual reset on last shot completion)
            if (currentShots <= 0)
            {
                Debug.Log("[Reset] Game ended during wait period. No further actions.");
                isResetting = false;
                yield break;
            }

            if (refereeWhistle != null)
            {
                Debug.Log("[Reset] Playing referee whistle: " + refereeWhistle.name);
                AudioSource.PlayClipAtPoint(refereeWhistle, Camera.main.transform.position, refereeWhistleVolume);
                yield return new WaitForSeconds(1f); // Wait for whistle to finish
            }
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
            strikerAnimator.Update(0f); // Ensure state is applied
            strikerAnimator.Play("Idle", 0, 0f); // Explicitly set to Idle
            Debug.Log("[Reset] Striker animator reset to Idle.");
        }
        yield return null; // Wait a frame for animator state to potentially settle

        // Trigger striker's kick animation
        if (strikerAnimator != null && kickerAnimationClip != null)
        {
            strikerAnimator.Play(kickerAnimationClip.name, 0, 0f);
            Debug.Log("[Reset] Striker kick animation triggered: " + kickerAnimationClip.name);
            // Optional: Wait for part of the animation if needed before shooting
            // yield return new WaitForSeconds(kickAnimationDelay);
        }
        else if (strikerAnimator == null || kickerAnimationClip == null)
        {
            Debug.LogWarning("[Reset] Missing striker animator or animation clip! Shooting immediately.");
        }
        yield return null; // Wait a frame

        ShootBall();
        shotsFired++; // Increment shots fired count *before* decrementing remaining
        currentShots--;
        Debug.Log($"[Reset] Shot cycle initiated. Shot {shotsFired}/{totalShots}. Shots remaining: {currentShots}");
        UpdateScoreUI();

        // IMPORTANT: Set isResetting to false AFTER shooting, allowing collisions to register for this shot.
        isResetting = false;

        // Check for game over condition AFTER the shot cycle is fully complete and isResetting is false
        if (currentShots <= 0)
        {
            // Wait a bit longer after the last shot to ensure collision events can process
            Debug.Log("[Game] All shots taken. Waiting a few seconds for final outcome...");
            yield return new WaitForSeconds(3f); // Give time for ball travel and potential last collision

            Debug.Log("[Game] Final wait complete. Returning to menu (Scene 0).");
            SceneManager.LoadScene(0); // Use SceneManager.LoadScene(sceneBuildIndex) or SceneManager.LoadScene("SceneName")
        }
    }

    // Collision detection for scoring, bouncing, etc.
    private void OnCollisionEnter(Collision collision)
    {
        if (!gameStarted)
            return;

        // Prevent processing collision if resetting for the *next* shot, but allow during current shot flight
        if (isResetting)
        {
             Debug.Log($"[Collision] Ignoring collision with {collision.gameObject.name} because isResetting is true.");
             return;
        }

        Debug.Log("[Collision] Ball collided with: " + collision.gameObject.name);

        // --- Collision Logic Order Matters ---

        // 1. Check for VR controller block (Highest priority for scoring)
        if (CheckControllersCollision(collision))
        {
            // Only score if not already scored this shot and didn't hit net first
            if (!hasScoredThisShot && !ballHitGoalNet)
            {
                Debug.Log("[Collision] Ball blocked by VR controller. Awarding point.");
                hasScoredThisShot = true;  // Set the flag immediately for this shot
                ballTouchedHandThisShot = true; // A controller block counts as touching a hand conceptually
                AddScore();
                if (blockedSound != null)
                {
                    Debug.Log("[Collision] Playing blocked sound: " + blockedSound.name);
                    AudioSource.PlayClipAtPoint(blockedSound, Camera.main.transform.position, blockedSoundVolume);
                }
                // Bounce the ball away after block
                if (enableCustomBounceBehavior)
                     BounceBall(collision); // Use the specific controller bounce

                // Start reset only if more shots are left
                if (currentShots > 0 && !isResetting)
                {
                    Debug.Log("[Collision] Controller block: Starting reset for next shot.");
                    StartCoroutine(ResetPositionsAndAnimate());
                }
                else if (currentShots <= 0)
                {
                     Debug.Log("[Collision] Controller block on LAST SHOT. Point awarded. Game will end.");
                     // The end game logic in ResetPositionsAndAnimate will handle scene transition.
                }
            } else {
                 Debug.Log($"[Collision] Ball hit VR controller, but already scored ({hasScoredThisShot}) or hit net ({ballHitGoalNet}). No action.");
                 // Optionally add a different sound/effect for post-score/post-net hits
            }
            return; // Handled controller collision
        }

        // 2. Check for Goalkeeper Hand collision (Second priority, marks the ball as touched)
        if (CheckGoalkeeperHandsCollision(collision))
        {
            Debug.Log("[Collision] Ball touched goalkeeper's hand.");
            ballTouchedHandThisShot = true; // *** Mark that hand was touched ***

            if (enableCustomBounceBehavior && enableGoalkeeperHandBounce())
            {
                BounceBallWithMultiplier(collision, handsBounceMultiplier);
            }

            // *** DO NOT RESET HERE - Let the ball continue to see if it goes in the net ***
            Debug.Log("[Collision] Hand touch registered. Ball continues trajectory.");
            // No return here, let execution continue to check other potential collisions (like net immediately after)
        }

        // 3. Check for Goal Net collision (Third priority)
        if (goalNet != null && (collision.gameObject == goalNet || collision.transform.IsChildOf(goalNet.transform)))
        {
            // *** NEW: Check if hand was touched *before* hitting the net ***
            if (ballTouchedHandThisShot)
            {
                // Hit hand first, then net. Considered blocked, no goal.
                Debug.Log("[Collision] Ball hit goal net AFTER hitting a hand. Considered blocked (no goal).");
                // Optionally play a soft thud sound here instead of goal sound
                // Do NOT set ballHitGoalNet = true;
            }
            else if (!hasScoredThisShot) // Only count as goal if not already saved by controller
            {
                // Hit net directly without prior hand/controller touch. Goal conceded.
                ballHitGoalNet = true; // Mark that it hit the net directly
                Debug.Log("[Collision] Ball hit the goal net directly. Goal conceded.");
                if (goalNetSound != null)
                {
                    Debug.Log("[Collision] Playing goal net sound: " + goalNetSound.name);
                    AudioSource.PlayClipAtPoint(goalNetSound, Camera.main.transform.position, goalNetSoundVolume);
                }
            } else {
                 Debug.Log("[Collision] Ball hit goal net, but already scored by controller block. Ignoring net hit.");
            }

            // Reset sequence should happen whether it was a direct goal or hand-then-goal, if shots remain
            if (currentShots > 0 && !isResetting)
            {
                 Debug.Log("[Collision] Net hit: Starting reset for next shot.");
                 StartCoroutine(ResetPositionsAndAnimate());
            }
             else if (currentShots <= 0)
            {
                 Debug.Log("[Collision] Net hit on LAST SHOT. Outcome determined (goal or blocked-then-net). Game will end.");
                 // The end game logic handles scene transition.
            }
            return; // Handled net collision
        }

        // 4. Check for Ground collision (Lowest priority action)
        if (groundObject != null && collision.gameObject == groundObject)
        {
            Debug.Log("[Collision] Ball hit the ground.");
            if (enableCustomBounceBehavior && enableGroundBounce())
            {
                BounceBallGround(collision);
            }
            // Do not trigger reset cycle on ground bounce.
            return; // Handled ground collision
        }

        // 5. Ignore collisions with the striker.
        if (collision.gameObject == kicker)
        {
            Debug.Log("[Collision] Ignoring collision with striker.");
            return;
        }

        // If collision is with none of the above, log it but take no action.
        // Debug.Log($"[Collision] Ball collided with unhandled object: {collision.gameObject.name}");
    }


    // Helper: Check if collision involves any VR controller.
    private bool CheckControllersCollision(Collision collision)
    {
        if (controllers == null || controllers.Length == 0) return false; // Safety check
        foreach (GameObject controller in controllers)
        {
            // Check direct collision and also if the collided object is a child of a controller (robustness)
            if (controller != null && (collision.gameObject == controller || collision.transform.IsChildOf(controller.transform)))
                return true;
        }
        return false;
    }

    // Helper: Check if collision involves any goalkeeper hand.
    private bool CheckGoalkeeperHandsCollision(Collision collision)
    {
        if (goalkeeperHands == null || goalkeeperHands.Length == 0) return false; // Safety check
        foreach (GameObject hand in goalkeeperHands)
        {
             // Check direct collision and also if the collided object is a child of a hand (robustness)
            if (hand != null && (collision.gameObject == hand || collision.transform.IsChildOf(hand.transform)))
                return true;
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
            // Use shotsFired which increments reliably when a shot is taken
            scoreText.text = $"Score: {score} | Shots: {shotsFired}/{totalShots}";
        else
            Debug.LogWarning("[BallPhysicsShooter] Score Text not assigned!");
    }

    // BounceBallGround: Reflect ball velocity on ground collision using groundBounceMultiplier and realistic energy loss.
    void BounceBallGround(Collision collision)
    {
        // Ensure rb.velocity is used if linearVelocity isn't updated correctly immediately after collision
        Vector3 incomingVelocity = rb.linearVelocity;
        Vector3 normal = collision.contacts[0].normal;
        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, normal);
        Vector3 newVelocity = reflectedVelocity * groundBounceMultiplier;

        if (newVelocity.magnitude < groundBounceThreshold)
        {
             newVelocity = Vector3.zero;
             rb.angularVelocity = Vector3.zero; // Stop spinning too
        }

        rb.linearVelocity = newVelocity; // Use velocity directly for better bounce control immediately after collision
        Debug.Log($"[BounceGround] Ball bounced off ground. Incoming Vel: {incomingVelocity.magnitude}, New Vel: {newVelocity.magnitude}");
    }

    // BounceBallWithMultiplier: Reflect ball velocity with given multiplier (for goalkeeper hands).
    void BounceBallWithMultiplier(Collision collision, float multiplier)
    {
        Vector3 incomingVelocity = rb.linearVelocity;
        Vector3 normal = collision.contacts[0].normal;
        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, normal);
        Vector3 newVelocity = reflectedVelocity * multiplier;

        rb.linearVelocity = newVelocity;
        Debug.Log($"[BounceHand] Ball bounced with multiplier {multiplier}. Incoming Vel: {incomingVelocity.magnitude}, New Vel: {newVelocity.magnitude}");
    }

    // BounceBall: Custom bounce for VR controller blocks using bounceMultiplier.
    void BounceBall(Collision collision)
    {
        Vector3 incomingVelocity = rb.linearVelocity; // Get current velocity
        Vector3 normal = collision.contacts[0].normal; // Get collision normal

        // Reflect the velocity off the normal
        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, normal);

        // Apply the bounce multiplier to the reflected velocity magnitude
        Vector3 bounceVelocity = reflectedVelocity.normalized * incomingVelocity.magnitude * bounceMultiplier;

        // Optional: Ensure some upward bounce if desired
        // bounceVelocity.y = Mathf.Max(bounceVelocity.y, incomingVelocity.magnitude * 0.1f); // Example: Ensure minimum upward component

        rb.linearVelocity = bounceVelocity; // Apply the final bounce velocity
        Debug.Log($"[BounceController] Ball bounced off controller. Incoming Vel: {incomingVelocity.magnitude}, New Vel: {bounceVelocity.magnitude} (Multiplier: {bounceMultiplier})");
    }


    // ShootBall: Shoot the ball toward a random goal target.
    void ShootBall()
    {
        if (goalTargets.Length == 0)
        {
            Debug.LogError("[Shoot] No goal targets assigned!");
            return;
        }
        // Stop any residual movement before applying new force
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Transform target = goalTargets[Random.Range(0, goalTargets.Length)];
        Vector3 directionToTarget = target.position - transform.position;

        // Calculate shot direction (you might want to adjust the Y component for arc)
        Vector3 shotDirection = directionToTarget.normalized;
        // Example: Add a slight upward angle based on distance or fixed value
        shotDirection.y += 0.1f + (directionToTarget.magnitude / 100f); // Adjust Y based on distance + base value
        shotDirection = shotDirection.normalized; // Re-normalize after adjusting Y

        Debug.Log("[Shoot] Ball shot toward: " + target.position + " with calculated force vector: " + (shotDirection * shotForce));
        // Apply force using AddForce with ForceMode.VelocityChange for immediate effect
        rb.AddForce(shotDirection * shotForce, ForceMode.VelocityChange);
    }

    // ResetBallPosition: Move the ball back to its initial position and zero its velocity.
    public void ResetBallPosition()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialBallPosition;
        Debug.Log("[Reset] Ball reset to initial position: " + initialBallPosition);
    }

    // ResetKickerTransform: Reset the striker's transform using the specified values.
    public void ResetKickerTransform()
    {
        if (kicker != null)
        {
            // Stop any physics movement if the kicker has a Rigidbody
            Rigidbody kickerRb = kicker.GetComponent<Rigidbody>();
            if (kickerRb != null)
            {
                kickerRb.linearVelocity = Vector3.zero;
                kickerRb.angularVelocity = Vector3.zero;
            }
            kicker.transform.position = kickerResetPosition;
            kicker.transform.rotation = Quaternion.Euler(kickerResetRotation);
            Debug.Log("[Reset] Kicker reset to position: " + kickerResetPosition + ", rotation: " + kickerResetRotation);
        }
    }

    // Feature toggle helper for ground bounce.
    private bool enableGroundBounce()
    {
        return enableCustomGroundBounce && groundObject != null;
    }

    // Feature toggle helper for goalkeeper hand bounce.
    private bool enableGoalkeeperHandBounce()
    {
        return enableCustomHandBounce && (goalkeeperHands != null && goalkeeperHands.Length > 0);
    }
}