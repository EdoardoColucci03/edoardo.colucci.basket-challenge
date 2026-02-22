using System.Collections;
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

    [Header("Stars")]
    [SerializeField] private Image[] stars;
    [SerializeField] private Color starActiveColor = new Color(1f, 0.82f, 0.40f);
    [SerializeField] private Color starInactiveColor = new Color(0.2f, 0.2f, 0.27f);

    [Header("Score Thresholds")]
    [SerializeField] private int[] thresholds = { 10, 20, 35, 50 };

    private readonly string[] titles = { "KEEP TRYING!", "NOT BAD!", "GOOD SHOT!", "SHARPSHOOTER!", "LEGENDARY!" };

    private void Start()
    {
        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        DisplayFinalScore();
    }

    private void DisplayFinalScore()
    {
        int score = GameManager.Instance != null ? GameManager.Instance.GetTotalScore() : 0;

        finalScoreText.text = score.ToString();

        int tier = 0;
        for (int i = 0; i < thresholds.Length; i++)
            if (score >= thresholds[i]) tier = i + 1;

        messageText.text = titles[tier];
        SetStars(tier + 1);
    }

    private void SetStars(int count)
    {
        for (int i = 0; i < stars.Length; i++)
            stars[i].color = i < count ? starActiveColor : starInactiveColor;
    }

    private void OnPlayAgainClicked() => GameManager.Instance?.PlayAgain();
    private void OnMainMenuClicked() => GameManager.Instance?.ReturnToMainMenu();
}
