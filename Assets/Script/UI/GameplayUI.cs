using System.Collections;
using UnityEngine;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance;

    [Header("UI Container")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;

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
    }

    private void Update()
    {
        UpdateTimerUI();
        UpdateScoreUI();
        UpdateBonusUI();
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


    public void ShowScoreFlyer(int points, string label, Color color)
    {
        if (flyerCoroutine != null) StopCoroutine(flyerCoroutine);
        flyerCoroutine = StartCoroutine(FlyerRoutine(points, label, color));
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
}