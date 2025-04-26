using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Make sure SceneTransitionManager is handled elsewhere or included if needed

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

    public List<Button> returnButtons;

    // You might need a reference to your SceneTransitionManager if it's not a singleton
    // public SceneTransitionManager sceneTransitionManager;

    // Start is called before the first frame update
    void Start()
    {
        EnableMainMenu();

        //Hook events
        startButton.onClick.AddListener(StartGame);
        optionButton.onClick.AddListener(EnableOption);
        aboutButton.onClick.AddListener(EnableAbout);
        quitButton.onClick.AddListener(QuitGame); // This already calls QuitGame

        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Requested!"); // Good to add a log

        // This preprocessor directive checks if we are running in the Unity Editor
        #if UNITY_EDITOR
            // If in the editor, stop playing the scene.
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("Stopping Play Mode in Editor.");
        #else
            // If in a build, quit the application.
            Application.Quit();
            Debug.Log("Application Quit requested (Build).");
        #endif
    }

    public void StartGame()
    {
        HideAll();
        // Make sure SceneTransitionManager is accessible, e.g., via a static singleton instance
        // If SceneTransitionManager.singleton doesn't exist, you'll need another way to reference it.
        if (SceneTransitionManager.singleton != null)
        {
             SceneTransitionManager.singleton.GoToSceneAsync(1);
        }
        else
        {
            Debug.LogError("SceneTransitionManager singleton not found! Cannot start game.");
            // Fallback or alternative scene loading if needed
            // SceneManager.LoadScene(1); // Example fallback
        }
    }

    public void HideAll()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (options != null) options.SetActive(false);
        if (about != null) about.SetActive(false);
    }

    public void EnableMainMenu()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (options != null) options.SetActive(false);
        if (about != null) about.SetActive(false);
    }
    public void EnableOption()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (options != null) options.SetActive(true);
        if (about != null) about.SetActive(false);
    }
    public void EnableAbout()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (options != null) options.SetActive(false);
        if (about != null) about.SetActive(true);
    }
}