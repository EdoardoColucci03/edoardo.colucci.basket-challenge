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
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Settings");
        AudioManager.Instance?.PlayButtonClickPrimary();
    }
}
