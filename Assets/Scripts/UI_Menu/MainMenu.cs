using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPausePanel;
    public GameObject settingsPanel;

    private CursorLockMode cursorLockStateBeforePause;
    private bool cursorVisibleBeforePause;
    private bool hasCursorStateBeforePause;

    public void PlayGame()
    {
        // If we are in the Main Menu scene (Scene 0 or named "MainMenu"), load the level.
        // Otherwise, if we are already in a level, just resume.
        if (SceneManager.GetActiveScene().name == "Main Menu") // Adjust name if needed
        {
            // No need to disable camera anymore as we are using a persistent system
            // and we want it to render throughout the transition.

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
        RestoreCursorStateAfterPause();
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
        EnsureUIInputUsesDynamicUpdate();
        UnlockCursorForMenu();
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

    private void UnlockCursorForMenu()
    {
        if (!hasCursorStateBeforePause)
        {
            cursorLockStateBeforePause = Cursor.lockState;
            cursorVisibleBeforePause = Cursor.visible;
            hasCursorStateBeforePause = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursorStateAfterPause()
    {
        if (!hasCursorStateBeforePause) return;

        Cursor.lockState = cursorLockStateBeforePause;
        Cursor.visible = cursorVisibleBeforePause;
        hasCursorStateBeforePause = false;
    }

    private void EnsureUIInputUsesDynamicUpdate()
    {
#if ENABLE_INPUT_SYSTEM
        if (InputSystem.settings != null &&
            InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
        {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        }
#endif
    }
}
