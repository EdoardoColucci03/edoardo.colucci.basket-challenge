using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float gameDuration = 60f;

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
        isGameActive = true;
        SceneManager.LoadScene("Gameplay");
    }

    private void EndGame()
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
}

