using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// You need this namespace for EditorApplication, but only include it in the editor
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject options;
    public GameObject about;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button optionButton;
    public Button aboutButton;
    public Button quitButton;

    // Assuming SceneTransitionManager is correctly set up elsewhere
    // public SceneTransitionManager sceneTransitionManager; // Make sure you have a reference if needed, maybe singleton pattern is used.

    public List<Button> returnButtons;

    // Start is called before the first frame update
    void Start()
    {
        EnableMainMenu();

        //Hook events
        startButton.onClick.AddListener(StartGame);
        optionButton.onClick.AddListener(EnableOption);
        aboutButton.onClick.AddListener(EnableAbout);
        quitButton.onClick.AddListener(QuitGame); // This listener is already correct

        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked!"); // Good for testing

        // Use preprocessor directives to handle editor vs. build
        #if UNITY_EDITOR
            // If we are running in the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("Stopping Play Mode in Editor.");
        #else
            // If we are running in a built game
            Application.Quit();
            Debug.Log("Quitting Application.");
        #endif
    }

    public void StartGame()
    {
        HideAll();
        // Ensure SceneTransitionManager and its singleton instance exist
        if (SceneTransitionManager.singleton != null)
        {
            SceneTransitionManager.singleton.GoToSceneAsync(1); // Assuming scene index 1 is your game scene
        }
        else
        {
            Debug.LogError("SceneTransitionManager singleton not found!");
            // Fallback or alternative scene loading if necessary
            // UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }

    public void HideAll()
    {
        mainMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(false);
    }

    public void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        options.SetActive(false);
        about.SetActive(false);
    }
    public void EnableOption()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
        about.SetActive(false);
    }
    public void EnableAbout()
    {
        mainMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(true);
    }
}