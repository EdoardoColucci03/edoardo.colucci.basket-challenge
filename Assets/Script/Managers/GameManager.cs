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
    private bool waitingForShotToFinish = false;

    private int shotsFiredCount = 0;
    private int aiShotsFiredCount = 0;
    private int nextBonusAtShot = 0;

    private BackboardBonus activeBonus = null;
    private bool isBonusActive = false;
    private float bonusTimeLeft = 0f;
    private Coroutine bonusExpireCoroutine;

    private GameMode currentGameMode = GameMode.Practice;
    private AIDifficulty currentDifficulty = AIDifficulty.Normal;
    private int aiScore = 0;

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

        if (!waitingForShotToFinish)
            timeRemaining -= Time.deltaTime;

        if (isBonusActive)
            bonusTimeLeft -= Time.deltaTime;

        if (timeRemaining <= 0f && !waitingForShotToFinish)
        {
            timeRemaining = 0f;

            if (PlayerController.Instance != null && PlayerController.Instance.IsBallInFlight)
            {
                waitingForShotToFinish = true;
            }
            else
            {
                EndGame();
            }
        }

        if (waitingForShotToFinish &&
            (PlayerController.Instance == null || !PlayerController.Instance.IsBallInFlight))
        {
            waitingForShotToFinish = false;
            EndGame();
        }
    }

    public void StartPractice(float duration = 60f)
    {
        currentGameMode = GameMode.Practice;
        gameDuration = duration;
        InitGame();
    }

    public void StartVsAI(AIDifficulty difficulty)
    {
        currentGameMode = GameMode.VsAI;
        currentDifficulty = difficulty;
        gameDuration = 120f;
        aiScore = 0;
        InitGame();
    }

    private void InitGame()
    {
        timeRemaining = gameDuration;
        totalScore = 0;
        shotsFiredCount = 0;
        aiShotsFiredCount = 0;
        waitingForShotToFinish = false;
        isGameActive = true;
        FireballManager.Instance?.ResetState();
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

    public void PlayAgain()
    {
        if (currentGameMode == GameMode.VsAI)
            StartVsAI(currentDifficulty);
        else
            StartPractice(gameDuration);
    }

    public void ReturnToModeSelection()
    {
        isGameActive = false;
        ClearBonus();
        SceneManager.LoadScene("ModeSelection");
    }

    public void OnShotFired()
    {
        if (!isGameActive) return;
        shotsFiredCount++;
        Debug.Log($"<color=cyan>[GM] Player shot #{shotsFiredCount} | Next bonus at #{nextBonusAtShot}</color>");
    }

    public void OnAIShotFired()
    {
        if (!isGameActive) return;
        aiShotsFiredCount++;
        Debug.Log($"<color=red>[GM] AI shot #{aiShotsFiredCount}</color>");
    }

    public void OnBallReady()
    {
        if (!isGameActive) return;
        if (isBonusActive) return;

        int totalShots = currentGameMode == GameMode.VsAI
            ? shotsFiredCount + aiShotsFiredCount
            : shotsFiredCount;

        if (totalShots >= nextBonusAtShot)
            SpawnBonus();
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        int finalPoints = FireballManager.Instance != null
            ? FireballManager.Instance.ApplyMultiplier(points)
            : points;
        totalScore += finalPoints;
        GameplayUI.Instance?.UpdateScore(totalScore);
    }

    public void OnPerfectShot()
    {
        AddScore(3);
        FireballManager.Instance?.OnBasketScored();
    }

    public void OnNormalBasket()
    {
        AddScore(2);
        FireballManager.Instance?.OnBasketScored();
    }

    public void OnBackboardBasket()
    {
        if (isBonusActive)
        {
            int bonusPoints = activeBonus.Points;
            AddScore(2 + bonusPoints);
            ClearBonus();
        }
        else
        {
            AddScore(2);
        }
        FireballManager.Instance?.OnBasketScored();
    }

    public void OnPlayerMiss()
    {
        FireballManager.Instance?.OnMiss();
    }

    public void OnAIBasket(int points)
    {
        if (!isGameActive) return;
        aiScore += points;
        GameplayUI.Instance?.ShowAIScore(aiScore);
    }

    public void OnAIBackboardBasket()
    {
        if (!isGameActive) return;

        int points = 2;

        if (isBonusActive)
        {
            points += activeBonus.Points;
            ClearBonus();
        }

        aiScore += points;
        GameplayUI.Instance?.ShowAIScore(aiScore);
        //Debug.Log($"<color=red>[AI] Backboard basket +{points}</color>");
    }

    private void ScheduleNextBonus()
    {
        int totalShots = currentGameMode == GameMode.VsAI
            ? shotsFiredCount + aiShotsFiredCount
            : shotsFiredCount;

        int interval = Random.Range(minShotsBeforeBonus, maxShotsBeforeBonus + 1);
        nextBonusAtShot = totalShots + interval;
        Debug.Log($"<color=yellow>[GM] Next bonus at total shot #{nextBonusAtShot}</color>");
    }

    private void SpawnBonus()
    {
        if (bonusExpireCoroutine != null)
            StopCoroutine(bonusExpireCoroutine);

        activeBonus = BackboardBonus.GetRandom();
        isBonusActive = true;
        bonusTimeLeft = bonusDuration;

        ScheduleNextBonus();

        GameplayUI.Instance?.ShowBonusNotification("BACKBOARD BONUS ACTIVE!", activeBonus.Color);
        AudioManager.Instance?.PlayBonusActive();

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
    public int GetAIScore() => aiScore;
    public float GetGameDuration() => gameDuration;
    public bool IsBonusActive => isBonusActive;
    public BackboardBonus ActiveBonus => activeBonus;
    public GameMode CurrentGameMode => currentGameMode;
    public AIDifficulty CurrentDifficulty => currentDifficulty;
}