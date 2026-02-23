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

    private void Start()
    {
        practiceButton.onClick.AddListener(OnPracticeClicked);
        vsAIButton.onClick.AddListener(OnVsAIClicked);
        backButton.onClick.AddListener(OnBackClicked);

        modeSelectionPanel.SetActive(true);
        difficultyPanel.SetActive(false);
    }

    private void OnPracticeClicked()
    {
        GameManager.Instance.StartPractice();
    }

    private void OnVsAIClicked()
    {
        modeSelectionPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        GameManager.Instance.ReturnToMainMenu();
    }
}
