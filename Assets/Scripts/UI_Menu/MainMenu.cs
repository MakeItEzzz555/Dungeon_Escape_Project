using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
