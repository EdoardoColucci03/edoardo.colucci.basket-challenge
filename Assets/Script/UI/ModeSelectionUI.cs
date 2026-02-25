using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectionUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button practiceButton;
    [SerializeField] private Button vsAIButton;
    [SerializeField] private Button backButton;

    [Header("Panels")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private GameObject timerSelectionPanel;

    [Header("Timer Buttons")]
    [SerializeField] private Button timer30sButton;
    [SerializeField] private Button timer60sButton;
    [SerializeField] private Button timer90sButton;
    [SerializeField] private Button timer120sButton;
    [SerializeField] private Button timerBackButton;

    private void Start()
    {
        practiceButton.onClick.AddListener(OnPracticeClicked);
        vsAIButton.onClick.AddListener(OnVsAIClicked);
        backButton.onClick.AddListener(OnBackClicked);

        timer30sButton.onClick.AddListener(() => OnTimerSelected(30));
        timer60sButton.onClick.AddListener(() => OnTimerSelected(60));
        timer90sButton.onClick.AddListener(() => OnTimerSelected(90));
        timer120sButton.onClick.AddListener(() => OnTimerSelected(120));
        timerBackButton.onClick.AddListener(OnTimerBackClicked);

        modeSelectionPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        timerSelectionPanel.SetActive(false);
    }

    private void OnPracticeClicked()
    {
        modeSelectionPanel.SetActive(false);
        timerSelectionPanel.SetActive(true);
    }

    private void OnVsAIClicked()
    {
        modeSelectionPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    private void OnTimerSelected(float seconds)
    {
        GameManager.Instance.StartPractice(seconds);
    }

    private void OnTimerBackClicked()
    {
        timerSelectionPanel.SetActive(false);
        modeSelectionPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        GameManager.Instance.ReturnToMainMenu();
    }
}
