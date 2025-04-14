using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Menu & HUD Panels")]
    [SerializeField] private GameObject menuPanel;   // The main menu panel (with Start and Quit buttons)
    [SerializeField] private GameObject gamePanel;   // The in-game HUD panel (for shot count display)

    [Header("Shot Settings")]
    [SerializeField] private int totalShots = 10;       // Total shots per game
    [SerializeField] private TextMeshProUGUI shotCountText; // UI text to display the remaining shots

    private int currentShots;

    void Awake()
    {
        Debug.Log("[GameManager] Awake called.");
        // Singleton pattern: ensure only one instance exists.
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[GameManager] Multiple instances detected. Destroying the new instance.");
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            Debug.Log("[GameManager] Instance assigned.");
        }
    }

    void Start()
    {
        Debug.Log("[GameManager] Start called.");
        ShowMenu();
    }

    // Called by the Start button on the menu.
    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame() called.");
        currentShots = totalShots;
        UpdateShotCountUI();
        menuPanel.SetActive(false);  // Hide the menu panel for the duration of the game.
        gamePanel.SetActive(true);   // Show the game HUD panel.
        Debug.Log("[GameManager] Game started. Shots remaining: " + currentShots);
    }

    // Called by the Quit button on the menu.
    public void QuitGame()
    {
        Debug.Log("[GameManager] QuitGame() called. Exiting game.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Called by BallPhysicsShooter each time a shot cycle finishes.
    public void ShotTaken()
    {
        currentShots--;
        Debug.Log("[GameManager] ShotTaken() called. New shot count: " + currentShots);
        UpdateShotCountUI();
        if (currentShots <= 0)
        {
            EndGame();
        }
    }

    private void UpdateShotCountUI()
    {
        if (shotCountText != null)
        {
            shotCountText.text = "Shots Remaining: " + currentShots;
            Debug.Log("[GameManager] Updated shot count UI: " + shotCountText.text);
        }
        else
        {
            Debug.LogWarning("[GameManager] shotCountText is not assigned in the Inspector.");
        }
    }

    // End the game and return to the menu.
    public void EndGame()
    {
        Debug.Log("[GameManager] EndGame() called. Game over.");
        menuPanel.SetActive(true);  // Show the menu panel again.
        gamePanel.SetActive(false); // Hide the in-game HUD.
    }

    // Show the initial menu.
    private void ShowMenu()
    {
        Debug.Log("[GameManager] Showing initial menu.");
        menuPanel.SetActive(true);
        gamePanel.SetActive(false);
    }
}
