using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Audio Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button creditsBackButton;

    private string previousScene;

    private void Start()
    {
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);

        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.wholeNumbers = false;
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;
        sfxSlider.wholeNumbers = false;

        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        creditsButton.onClick.AddListener(OnCreditsClicked);
        previousScene = PlayerPrefs.GetString("PreviousScene", "MainMenu");
        settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        creditsBackButton.onClick.AddListener(OnCreditsBackClicked);

        LoadVolumes();
    }

    private void LoadVolumes()
    {
        if (AudioManager.Instance != null)
        {
            float musicVol = Mathf.Round(AudioManager.Instance.musicVolume * 10f) / 10f;
            float sfxVol = Mathf.Round(AudioManager.Instance.sfxVolume * 10f) / 10f;
            musicSlider.value = musicVol;
            sfxSlider.value = sfxVol;
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        float steppedValue = Mathf.Round(value * 10f) / 10f;
        musicSlider.value = steppedValue;
        AudioManager.Instance?.SetMusicVolume(steppedValue);
    }

    private void OnSFXSliderChanged(float value)
    {
        float steppedValue = Mathf.Round(value * 10f) / 10f;
        sfxSlider.value = steppedValue;
        AudioManager.Instance?.SetSFXVolume(steppedValue);
    }

    private void OnCreditsClicked()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    private void OnSettingsBackClicked()
    {
        AudioManager.Instance?.SaveVolumes();
        SceneManager.LoadScene(previousScene);
    }

    private void OnCreditsBackClicked()
    {
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
}
