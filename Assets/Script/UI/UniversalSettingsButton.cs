using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UniversalSettingsButton : MonoBehaviour
{
    [SerializeField] private Button settingsButton;

    private void Start()
    {
        settingsButton.onClick.AddListener(OpenSettings);
    }

    private void OpenSettings()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "MainMenu")
        {
            PlayerPrefs.SetString("PreviousScene", currentScene);
        }

        SceneManager.LoadScene("Settings");
        AudioManager.Instance?.PlayButtonClickPrimary();
    }
}
