using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamplayUI : MonoBehaviour
{
    [Header("UI Container")]
    [SerializeField] private TextMeshProUGUI timerText;

    private void Update()
    {
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        float time = GameManager.Instance.GetTimeRemaining();
        int seconds = Mathf.CeilToInt(time);
        timerText.text = $"Time: {seconds}s";
    }

}
