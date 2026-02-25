using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject practicePanel;
    [SerializeField] private GameObject vsAIPanel;

    [Header("Practice References")]
    [SerializeField] private TextMeshProUGUI practiceFinalScoreText;
    [SerializeField] private TextMeshProUGUI practiceMessageText;
    [SerializeField] private Image[] stars;
    [SerializeField] private Color starActiveColor = new Color(1f, 0.82f, 0.40f);
    [SerializeField] private Color starInactiveColor = new Color(0.2f, 0.2f, 0.27f);

    [Header("VS AI References")]
    [SerializeField] private TextMeshProUGUI vsResultText;
    [SerializeField] private TextMeshProUGUI vsPlayerScoreText;
    [SerializeField] private TextMeshProUGUI vsAIScoreText;
    [SerializeField] private TextMeshProUGUI vsMessageText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button modeSelectionButton;

    [Header("Score Thresholds")]
    [SerializeField] private int[] thresholds = { 10, 20, 35, 50 };

    private readonly string[] titles = { "KEEP TRYING!", "NOT BAD!", "GOOD SHOT!", "SHARPSHOOTER!", "LEGENDARY!" };

    private void Start()
    {
        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        modeSelectionButton.onClick.AddListener(OnModeSelectionClicked);

        StartCoroutine(DisplayAfterFrame());
    }

    private IEnumerator DisplayAfterFrame()
    {
        yield return null;

        bool playSounds = !GameManager.Instance.HasPlayedRewardSound;
        GameManager.Instance.HasPlayedRewardSound = true;

        DisplayFinalScore(playSounds);
    }

    private void DisplayFinalScore(bool playSounds)
    {
        int score = GameManager.Instance != null ? GameManager.Instance.GetTotalScore() : 0;
        bool isVsAI = GameManager.Instance != null && GameManager.Instance.CurrentGameMode == GameMode.VsAI;

        practicePanel.SetActive(!isVsAI);
        vsAIPanel.SetActive(isVsAI);

        if (isVsAI)
        {
            int aiScore = GameManager.Instance.GetAIScore();
            vsPlayerScoreText.text = score.ToString();
            vsAIScoreText.text = aiScore.ToString();

            if (score > aiScore)
            {
                vsResultText.text = "YOU WIN!";
                vsResultText.color = new Color(1f, 0.5f, 0f);
                vsMessageText.text = "Well played!";
                if (playSounds) AudioManager.Instance?.PlayWin();
            }
            else if (score < aiScore)
            {
                vsResultText.text = "YOU LOSE!";
                vsResultText.color = Color.magenta;
                vsMessageText.text = "Better luck next time!";
                if (playSounds) AudioManager.Instance?.PlayLose();
            }
            else
            {
                vsResultText.text = "DRAW!";
                vsResultText.color = Color.yellow;
                vsMessageText.text = "So close!";
                if (playSounds) AudioManager.Instance?.PlayDraw();
            }
        }
        else
        {
            float duration = GameManager.Instance != null ? GameManager.Instance.GetGameDuration() : 120f;
            float scale = duration / 120f;

            int tier = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                int scaledThreshold = Mathf.RoundToInt(thresholds[i] * scale);
                if (score >= scaledThreshold) tier = i + 1;
            }

            practiceFinalScoreText.text = score.ToString();
            practiceMessageText.text = titles[tier];
            SetStars(tier + 1, playSounds);
        }
    }

    private void SetStars(int count, bool playSounds)
    {
        for (int i = 0; i < stars.Length; i++)
            stars[i].color = i < count ? starActiveColor : starInactiveColor;

        if (playSounds) AudioManager.Instance?.PlayStarsSound(count);
    }

    private void OnPlayAgainClicked() { GameManager.Instance?.PlayAgain(); }
    private void OnModeSelectionClicked() { GameManager.Instance?.ReturnToModeSelection(); }
    private void OnMainMenuClicked() { GameManager.Instance?.ReturnToMainMenu(); }
}