using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Container")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void OnPlayButtonClicked()
    {
        PlayerPrefs.DeleteKey("PreviousScene");
        SceneManager.LoadScene("ModeSelection");
    }

    public void OnSettingsButtonClicked()
    {
        PlayerPrefs.DeleteKey("PreviousScene");
        SceneManager.LoadScene("Settings");
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }

}
