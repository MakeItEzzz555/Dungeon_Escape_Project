using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scripts.Managers;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    private const string MainMenuSceneName = "Main Menu";
    private const string FirstLevelSceneName = "Level 1";

    [Header("Menu Panels")]
    public GameObject mainPausePanel;
    public GameObject settingsPanel;

    private CursorLockMode cursorLockStateBeforePause;
    private bool cursorVisibleBeforePause;
    private bool hasCursorStateBeforePause;

    private void Awake()
    {
        BindPauseMenuButtons();
    }

    private void OnEnable()
    {
        BindPauseMenuButtons();
    }

    public void PlayGame()
    {
        // If we are in the Main Menu scene (Scene 0 or named "MainMenu"), load the level.
        // Otherwise, if we are already in a level, just resume.
        if (SceneManager.GetActiveScene().name == MainMenuSceneName)
        {
            Time.timeScale = 1f;
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.StartGameFromMainMenu(FirstLevelSceneName);
                return;
            }

            SceneManager.LoadScene(FirstLevelSceneName);
            AudioManager.Instance?.PlayGameplayMusic();
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
        HUDManager.Instance?.SetGameplayHudSuppressed(false);
        Time.timeScale = 1f;
        RestoreCursorStateAfterPause();
    }

    public void PauseGame()
    {
        // First ensure the GameObject itself is active
        gameObject.SetActive(true);
        if (transform.parent != null)
        {
            transform.SetAsLastSibling();
        }
        HUDManager.Instance?.SetGameplayHudSuppressed(true);
        
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
            mainPausePanel.transform.SetAsLastSibling();
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
        if (SceneManager.GetActiveScene().name != MainMenuSceneName)
        {
            ReturnToMainMenu();
            return;
        }

        Debug.Log("[DEBUG_LOG] MainMenu: Quit requested.");
        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("[DEBUG_LOG] MainMenu: Returning to Main Menu.");

        Time.timeScale = 1f;
        HUDManager.Instance?.SetGameplayHudSuppressed(false);
        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        hasCursorStateBeforePause = false;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ExitToMainMenuFromGameplay();
            return;
        }

        SceneManager.LoadScene(MainMenuSceneName);
        AudioManager.Instance?.PlayMainMenuMusic();
    }

    public void OpenSettings()
    {
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }
    }

    public void CloseSettings()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
            mainPausePanel.transform.SetAsLastSibling();
        }
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

    private void BindPauseMenuButtons()
    {
        if (mainPausePanel == null) return;

        Button[] buttons = mainPausePanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name != "Exit_bttn") continue;

            if (HasPersistentQuitGameBinding(button)) continue;

            button.onClick.RemoveListener(QuitGame);
            button.onClick.AddListener(QuitGame);
        }
    }

    private bool HasPersistentQuitGameBinding(Button button)
    {
        int eventCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < eventCount; i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == nameof(QuitGame))
            {
                return true;
            }
        }

        return false;
    }
}
