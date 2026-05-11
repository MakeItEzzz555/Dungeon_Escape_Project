using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPausePanel;
    public GameObject settingsPanel;

    public void PlayGame()
    {
        // If we are in the Main Menu scene (Scene 0 or named "MainMenu"), load the level.
        // Otherwise, if we are already in a level, just resume.
        if (SceneManager.GetActiveScene().name == "Main Menu") // Adjust name if needed
        {
            // Effect the main camera: disable its component or its GameObject 
            // to stop it from rendering/functioning before the scene transition.
            if (Camera.main != null)
            {
                Camera.main.enabled = false;
            }

            // Load the first level of the game (Level 1)
            SceneManager.LoadScene("Level 1");

            // Update Music to gameplay
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameplayMusic();
            }
        }
        else
        {
            // Resume the game
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        // First ensure the GameObject itself is active
        gameObject.SetActive(true);
        
        // Ensure the Canvas component is also enabled if it exists
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = true;
        }

        Debug.Log($"[DEBUG_LOG] MainMenu: PauseGame called. GameObject: {gameObject.name}");
        
        // Ensure the Main Pause Panel is active and Settings is inactive when opening
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
            Debug.Log("[DEBUG_LOG] MainMenu: mainPausePanel activated via reference.");
        }
        else
        {
            // Fallback: Try to find a child named "Main Menu" or similar if reference is missing
            Transform child = transform.Find("Main Menu");
            if (child == null) child = transform.Find("MainMenu");
            if (child == null) child = transform.Find("PauseMenu");
            if (child == null) child = transform.Find("Pause Menu");
            
            if (child != null)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"[DEBUG_LOG] MainMenu: '{child.name}' child activated via search.");
            }
            else if (transform.childCount > 0)
            {
                // Last resort: activate the first child if it's likely the main panel
                Transform firstChild = transform.GetChild(0);
                firstChild.gameObject.SetActive(true);
                Debug.Log($"[DEBUG_LOG] MainMenu: No named child found. Activating first child: {firstChild.name}");
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] MainMenu: No children found to activate!");
            }
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("[DEBUG_LOG] MainMenu: settingsPanel deactivated via reference.");
        }
        else
        {
            Transform child = transform.Find("Settings Menu");
            if (child == null) child = transform.Find("SettingsMenu");
            if (child != null)
            {
                child.gameObject.SetActive(false);
                Debug.Log($"[DEBUG_LOG] MainMenu: '{child.name}' child deactivated via search.");
            }
        }
        
        Time.timeScale = 0f;
        Debug.Log("[DEBUG_LOG] MainMenu: Game Paused. GameObject, Canvas, and Panels updated.");
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    public void OpenSettings()
    {
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (mainPausePanel != null) mainPausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
