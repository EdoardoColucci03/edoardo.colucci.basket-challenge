using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance;

    [Header("UI Container")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("VS AI")]
    [SerializeField] private GameObject aiScoreContainer;
    [SerializeField] private TextMeshProUGUI aiScoreText;
    [SerializeField] private RectTransform timerRect;

    [Header("Backboard Bonus 3D")]
    [SerializeField] private TextMeshPro bonusText3D;
    [SerializeField] private float bonusWarningTime = 3f;
    [SerializeField] private float pulseSpeed = 8f;

    [Header("Score Flyer")]
    [SerializeField] private TextMeshProUGUI flyerText;
    [SerializeField] private float flyerRiseSpeed = 80f;
    [SerializeField] private float flyerDuration = 1.2f;

    [Header("Bonus Notification")]
    [SerializeField] private TextMeshProUGUI bonusNotificationText;
    [SerializeField] private float notificationDuration = 2f;
    [SerializeField] private float notificationFadeDuration = 0.5f;

    [Header("Fireball UI")]
    [SerializeField] private GameObject fireballContainer;
    [SerializeField] private RectTransform fireballBarFill;
    [SerializeField] private Image fireballBarFillImage;
    [SerializeField] private Image fireballEffectImage;
    [SerializeField] private GameObject fireballActiveEffect;
    [SerializeField] private TextMeshProUGUI fireballActiveText;
    [SerializeField] private float fireballPulseSpeed = 6f;

    private readonly Color fillColorEmpty = new Color(1f, 0.85f, 0f, 1f);
    private readonly Color fillColorMid = new Color(1f, 0.45f, 0f, 1f);
    private readonly Color fillColorFull = new Color(1f, 0.05f, 0f, 1f);

    private readonly Color effectColorDim = new Color(0.2f, 0.05f, 0f, 0.4f);
    private readonly Color effectColorFull = new Color(1f, 0.05f, 0f, 1f);

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button backToGameButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button audioSettingsButton;
    [SerializeField] private GameObject audioSubPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button audioBackButton;

    private bool isPaused = false;
    private Coroutine flyerCoroutine;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        flyerText.gameObject.SetActive(false);
        bonusNotificationText.gameObject.SetActive(false);
        pausePanel.SetActive(false);

        if (fireballEffectImage != null)
            fireballEffectImage.color = effectColorDim;
        if (fireballBarFillImage != null)
            fireballBarFillImage.color = fillColorEmpty;

        pauseButton.onClick.AddListener(OpenPause);
        backToGameButton.onClick.AddListener(ClosePause);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        restartButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance?.PlayAgain();
        });

        audioSettingsButton.onClick.AddListener(OnAudioSettingsClicked);
        audioBackButton.onClick.AddListener(OnAudioBackClicked);

        SetupAudioSliders();

        bool isVsAI = GameManager.Instance != null &&
                      GameManager.Instance.CurrentGameMode == GameMode.VsAI;

        if (aiScoreContainer != null)
            aiScoreContainer.SetActive(isVsAI);

        if (isVsAI && aiScoreText != null)
            aiScoreText.text = "ScoreAI: 0";

        if (!isVsAI && timerRect != null && aiScoreContainer != null)
        {
            RectTransform aiRect = aiScoreContainer.GetComponent<RectTransform>();
            timerRect.anchorMin = aiRect.anchorMin;
            timerRect.anchorMax = aiRect.anchorMax;
            timerRect.pivot = aiRect.pivot;
            timerRect.anchoredPosition = aiRect.anchoredPosition;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ClosePause();
            else OpenPause();
        }

        if (!isPaused)
        {
            UpdateTimerUI();
            UpdateScoreUI();
            UpdateBonusUI();
            UpdateFireballUI();
        }
    }

    private void OpenPause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ClosePause()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void UpdateTimerUI()
    {
        float time = GameManager.Instance.GetTimeRemaining();
        int totalSeconds = Mathf.CeilToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes}:{seconds:D2}";
        timerText.color = totalSeconds <= 10 ? Color.red : Color.yellow;
    }

    private void UpdateScoreUI()
    {
        int score = GameManager.Instance.GetTotalScore();
        scoreText.text = $"Score: {score}";
    }

    private void UpdateBonusUI()
    {
        if (GameManager.Instance.IsBonusActive)
        {
            BackboardBonus bonus = GameManager.Instance.ActiveBonus;
            bonusText3D.gameObject.SetActive(true);
            bonusText3D.text = bonus.Label;

            float remaining = GameManager.Instance.GetBonusTimeRemaining();

            if (remaining <= bonusWarningTime)
            {
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                bonusText3D.color = new Color(bonus.Color.r, bonus.Color.g, bonus.Color.b, alpha);
            }
            else
            {
                bonusText3D.color = bonus.Color;
            }
        }
        else
        {
            bonusText3D.gameObject.SetActive(false);
        }
    }

    private void UpdateFireballUI()
    {
        bool isActive = FireballManager.Instance != null && FireballManager.Instance.IsFireballActive;

        if (fireballActiveText != null)
        {
            fireballActiveText.gameObject.SetActive(isActive);
            if (isActive)
            {
                float alpha = (Mathf.Sin(Time.time * fireballPulseSpeed) + 1f) / 2f;
                fireballActiveText.color = new Color(1f, 0.4f, 0f, Mathf.Lerp(0.6f, 1f, alpha));
                fireballActiveText.text = "FIREBALL MODE ACTIVE!";
            }
        }

        if (isActive)
        {
            float pulse = (Mathf.Sin(Time.time * fireballPulseSpeed) + 1f) / 2f;

            if (fireballEffectImage != null)
                fireballEffectImage.color = Color.Lerp(effectColorFull * 0.6f, effectColorFull, pulse);

            if (fireballBarFillImage != null)
                fireballBarFillImage.color = Color.Lerp(fillColorFull * 0.6f, fillColorFull, pulse);
        }
    }

    public void ShowAIScore(int score)
    {
        if (aiScoreText != null)
            aiScoreText.text = $"ScoreAI: {score}";
    }

    public void ShowScoreFlyer(int points, string label, Color color, bool fireballActive = false)
    {
        int displayPoints = fireballActive ? points * 2 : points;
        Color displayColor = fireballActive ? new Color(1f, 0.4f, 0f) : color;
        string displayLabel = fireballActive ? $"FIREBALL! {label}" : label;

        if (flyerCoroutine != null) StopCoroutine(flyerCoroutine);
        flyerCoroutine = StartCoroutine(FlyerRoutine(displayPoints, displayLabel, displayColor));
    }

    private IEnumerator FlyerRoutine(int points, string label, Color color)
    {
        flyerText.gameObject.SetActive(true);
        flyerText.text = $"{label}\n+{points}";
        flyerText.color = color;

        Vector3 startPos = flyerText.rectTransform.anchoredPosition3D;
        float elapsed = 0f;

        while (elapsed < flyerDuration)
        {
            float t = elapsed / flyerDuration;
            flyerText.rectTransform.anchoredPosition3D = startPos + Vector3.up * flyerRiseSpeed * t;
            float alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
            flyerText.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flyerText.rectTransform.anchoredPosition3D = startPos;
        flyerText.gameObject.SetActive(false);
    }

    public void ShowBonusNotification(string message, Color color)
    {
        if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
        notificationCoroutine = StartCoroutine(NotificationRoutine(message, color));
    }

    public void UpdateFireballBar(int current, int total)
    {
        if (fireballBarFill == null) return;

        bool isActive = FireballManager.Instance != null && FireballManager.Instance.IsFireballActive;
        float normalized = isActive ? 1f : Mathf.Clamp01((float)current / total);

        fireballBarFill.anchorMin = new Vector2(0, 0);
        fireballBarFill.anchorMax = new Vector2(1, normalized);
        fireballBarFill.offsetMin = Vector2.zero;
        fireballBarFill.offsetMax = Vector2.zero;

        if (fireballBarFillImage != null)
        {
            Color fillColor = normalized < 0.5f
                ? Color.Lerp(fillColorEmpty, fillColorMid, normalized * 2f)
                : Color.Lerp(fillColorMid, fillColorFull, (normalized - 0.5f) * 2f);
            fireballBarFillImage.color = fillColor;
        }

        if (fireballEffectImage != null && !isActive)
            fireballEffectImage.color = Color.Lerp(effectColorDim, effectColorFull * 0.6f, normalized);
    }

    public void ShowFireball(bool active)
    {
        if (active)
        {
            if (fireballBarFill != null)
            {
                fireballBarFill.anchorMin = new Vector2(0, 0);
                fireballBarFill.anchorMax = new Vector2(1, 1);
                fireballBarFill.offsetMin = Vector2.zero;
                fireballBarFill.offsetMax = Vector2.zero;
            }

            if (fireballBarFillImage != null)
                fireballBarFillImage.color = fillColorFull;
        }
        else
        {
            if (fireballBarFill != null)
            {
                fireballBarFill.anchorMin = new Vector2(0, 0);
                fireballBarFill.anchorMax = new Vector2(1, 0);
                fireballBarFill.offsetMin = Vector2.zero;
                fireballBarFill.offsetMax = Vector2.zero;
            }
            if (fireballBarFillImage != null)
                fireballBarFillImage.color = fillColorEmpty;
            if (fireballEffectImage != null)
                fireballEffectImage.color = effectColorDim;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private IEnumerator NotificationRoutine(string message, Color color)
    {
        bonusNotificationText.gameObject.SetActive(true);
        bonusNotificationText.text = message;
        bonusNotificationText.color = color;

        yield return new WaitForSeconds(notificationDuration);

        float elapsed = 0f;
        while (elapsed < notificationFadeDuration)
        {
            float alpha = 1f - (elapsed / notificationFadeDuration);
            bonusNotificationText.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bonusNotificationText.gameObject.SetActive(false);
    }

    private void SetupAudioSliders()
    {
        musicSlider.minValue = 0f; musicSlider.maxValue = 1f; musicSlider.wholeNumbers = false;
        sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; sfxSlider.wholeNumbers = false;

        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        LoadVolumes();
    }

    private void LoadVolumes()
    {
        if (AudioManager.Instance == null) return;

        float musicVol = Mathf.Round(AudioManager.Instance.musicVolume * 10f) / 10f;
        float sfxVol = Mathf.Round(AudioManager.Instance.sfxVolume * 10f) / 10f;
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
    }

    private void OnMusicSliderChanged(float value)
    {
        float stepped = Mathf.Round(value * 10f) / 10f;
        musicSlider.value = stepped;
        AudioManager.Instance?.SetMusicVolume(stepped);
    }

    private void OnSFXSliderChanged(float value)
    {
        float stepped = Mathf.Round(value * 10f) / 10f;
        sfxSlider.value = stepped;
        AudioManager.Instance?.SetSFXVolume(stepped);
    }

    public void OnAudioSettingsClicked()
    {
        buttonContainer.SetActive(false);
        audioSubPanel.SetActive(true);
    }

    public void OnAudioBackClicked()
    {
        audioSubPanel.SetActive(false);
        buttonContainer.SetActive(true);
    }
}