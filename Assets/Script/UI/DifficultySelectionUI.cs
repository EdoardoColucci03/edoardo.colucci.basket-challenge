using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultySelectionUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button backButton;
    [SerializeField] private ModeSelectionUI modeSelectionUI;

    private void Start()
    {
        easyButton.onClick.AddListener(() => OnDifficultySelected(AIDifficulty.Easy));
        normalButton.onClick.AddListener(() => OnDifficultySelected(AIDifficulty.Normal));
        hardButton.onClick.AddListener(() => OnDifficultySelected(AIDifficulty.Hard));
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDifficultySelected(AIDifficulty difficulty)
    {
        GameManager.Instance.StartVsAI(difficulty);
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);
        modeSelectionUI.gameObject.SetActive(true);
    }

}
