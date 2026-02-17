using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;

    private int totalScore = 0;
    private float timeRemaining;
    private bool isGameActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isGameActive || SceneManager.GetActiveScene().name != "Gameplay")
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }
    }

    public void StartGame()
    {
        timeRemaining = gameDuration;
        totalScore = 0;
        isGameActive = true;
        SceneManager.LoadScene("Gameplay");
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        totalScore += points;
        Debug.Log($"<color=green>+{points} points! Total: {totalScore}</color>");
    }

    public void OnPerfectShot()
    {
        AddScore(3);
    }

    public void OnNormalBasket()
    {
        AddScore(2);
    }

    public void EndGame()
    {
        isGameActive = false;
        SceneManager.LoadScene("Reward");
    }

    public void ReturnToMainMenu()
    {
        isGameActive = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain()
    {
        StartGame();
    }

    public float GetTimeRemaining() => timeRemaining;
    public int GetTotalScore() => totalScore;
}

