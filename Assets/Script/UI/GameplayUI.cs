using System.Collections;
using UnityEngine;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [Header("UI Container")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Backboard Bonus 3D")]
    [SerializeField] private TextMeshPro bonusText3D;

    private void Update()
    {
        UpdateTimerUI();
        UpdateScoreUI();
        UpdateBonusUI();
    }

    private void UpdateTimerUI()
    {
        float time = GameManager.Instance.GetTimeRemaining();
        int seconds = Mathf.CeilToInt(time);
        timerText.text = $"Time: {seconds}s";
        timerText.color = seconds <= 10 ? Color.red : Color.white;
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
            bonusText3D.color = bonus.Color;
        }
        else
        {
            bonusText3D.gameObject.SetActive(false);
        }
    }
}