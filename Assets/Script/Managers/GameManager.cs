using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;

    [Header("Backboard Bonus Settings")]
    [SerializeField] private int minShotsBeforeBonus = 5;
    [SerializeField] private int maxShotsBeforeBonus = 8;
    [SerializeField] private float bonusDuration = 8f;

    private int totalScore = 0;
    private float timeRemaining;
    private bool isGameActive;

    private int shotsFiredCount = 0;
    private int nextBonusAtShot = 0;

    private BackboardBonus activeBonus = null;
    private bool isBonusActive = false;
    private float bonusTimeLeft = 0f;
    private Coroutine bonusExpireCoroutine;

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

        if (isBonusActive)
            bonusTimeLeft -= Time.deltaTime;
    }

    public void StartGame()
    {
        timeRemaining = gameDuration;
        totalScore = 0;
        shotsFiredCount = 0;
        isGameActive = true;
        ClearBonus();
        ScheduleNextBonus();
        SceneManager.LoadScene("Gameplay");
    }

    public void EndGame()
    {
        isGameActive = false;
        ClearBonus();
        SceneManager.LoadScene("Reward");
    }

    public void ReturnToMainMenu()
    {
        isGameActive = false;
        ClearBonus();
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain() => StartGame();

    public void OnShotFired()
    {
        if (!isGameActive) return;

        shotsFiredCount++;

        Debug.Log($"<color=cyan>[GM] Shot fired #{shotsFiredCount} | Next bonus at #{nextBonusAtShot}</color>");
    }

    public void OnBallReady()
    {
        if (!isGameActive) return;

        if (isBonusActive) return;

        if (shotsFiredCount >= nextBonusAtShot)
            SpawnBonus();
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;
        totalScore += points;
        //Debug.Log($"<color=green>+{points} points! Total: {totalScore}</color>");
    }

    public void OnPerfectShot()
    {
        AddScore(3);
    }

    public void OnNormalBasket()
    {
        AddScore(2);
    }

    public void OnBackboardBasket()
    {
        if (isBonusActive)
        {
            int bonusPoints = activeBonus.Points;
            AddScore(2 + bonusPoints);
            //Debug.Log($"<color=orange>BACKBOARD BONUS collected! +{bonusPoints} | Rarity: {activeBonus.Rarity}</color>");
            ClearBonus();
        }
        else
        {
            AddScore(2);
        }
    }

    private void ScheduleNextBonus()
    {
        int interval = Random.Range(minShotsBeforeBonus, maxShotsBeforeBonus + 1);
        nextBonusAtShot = shotsFiredCount + interval;
        Debug.Log($"<color=yellow>[GM] Next bonus at shot #{nextBonusAtShot} (in {interval} shots)</color>");
    }

    private void SpawnBonus()
    {
        if (bonusExpireCoroutine != null)
            StopCoroutine(bonusExpireCoroutine);

        activeBonus = BackboardBonus.GetRandom();
        isBonusActive = true;
        bonusTimeLeft = bonusDuration;

        ScheduleNextBonus();

        //Debug.Log($"<color=yellow>[GM] BONUS spawned: {activeBonus.Rarity} +{activeBonus.Points} | Expires in {bonusDuration}s</color>");

        GameplayUI.Instance?.ShowBonusNotification($"BACKBOARD BONUS ACTIVE!", activeBonus.Color);

        bonusExpireCoroutine = StartCoroutine(BonusExpireRoutine());
    }

    private IEnumerator BonusExpireRoutine()
    {
        yield return new WaitForSeconds(bonusDuration);
        Debug.Log("<color=grey>[GM] Bonus expired</color>");
        ClearBonus();
    }

    private void ClearBonus()
    {
        if (bonusExpireCoroutine != null)
        {
            StopCoroutine(bonusExpireCoroutine);
            bonusExpireCoroutine = null;
        }

        activeBonus = null;
        isBonusActive = false;
        bonusTimeLeft = 0f;
    }

    public float GetTimeRemaining() => timeRemaining;
    public float GetBonusTimeRemaining() => bonusTimeLeft;
    public int GetTotalScore() => totalScore;
    public bool IsBonusActive => isBonusActive;
    public BackboardBonus ActiveBonus => activeBonus;
}