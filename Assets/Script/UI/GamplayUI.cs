using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamplayUI : MonoBehaviour
{
    [Header("UI Container")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;   

    private void Update()
    {
        UpdateTimerUI();
        UpdateScoreUI();
    }

    private void UpdateTimerUI()
    {
        float time = GameManager.Instance.GetTimeRemaining();
        int seconds = Mathf.CeilToInt(time);
        timerText.text = $"Time: {seconds}s";
    }

    private void UpdateScoreUI()
    {
        int score = GameManager.Instance.GetTotalScore();
        scoreText.text = $"Score: {score}";
    }
}
