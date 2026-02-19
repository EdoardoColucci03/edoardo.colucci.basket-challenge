using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Score Messages")]
    [SerializeField] private string excellentMessage = "EXCELLENT!";
    [SerializeField] private string greatMessage = "GREAT JOB!";
    [SerializeField] private string goodMessage = "GOOD!";
    [SerializeField] private string tryAgainMessage = "TRY AGAIN!";

    [Header("Score Thresholds")]
    [SerializeField] private int excellentThreshold = 50;
    [SerializeField] private int greatThreshold = 30;
    [SerializeField] private int goodThreshold = 15;

    private void Start()
    {
        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        DisplayFinalScore();
    }

    private void DisplayFinalScore()
    {
        if (GameManager.Instance == null)
        {
            finalScoreText.text = "Score: 0";
            messageText.text = tryAgainMessage;
            return;
        }

        int finalScore = GameManager.Instance.GetTotalScore();
        finalScoreText.text = $"FINAL SCORE: {finalScore}";

        string message = GetMessageForScore(finalScore);
        messageText.text = message;

        //Debug.Log($"<color=yellow>Game ended with score: {finalScore}</color>");
    }

    private string GetMessageForScore(int score)
    {
        if (score >= excellentThreshold)
            return excellentMessage;
        else if (score >= greatThreshold)
            return greatMessage;
        else if (score >= goodThreshold)
            return goodMessage;
        else
            return tryAgainMessage;
    }

    private void OnPlayAgainClicked()
    {
        GameManager.Instance?.PlayAgain();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }
}

