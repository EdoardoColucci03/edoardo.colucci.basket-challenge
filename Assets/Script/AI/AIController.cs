using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Ball Reference")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("Position Management")]
    [SerializeField] private SpawnPositionManager spawnManager;
    [SerializeField] private Transform aiTransform;

    [Header("Basket Reference")]
    [SerializeField] private Transform basketTarget;
    [SerializeField] private Transform aiBackboardTarget;
    [SerializeField] private AIBasketDetector basketDetector;

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    [Header("Timing")]
    [SerializeField] private float shootDelay = 2f;
    [SerializeField] private float resetDelay = 2f;

    private GameObject currentBall;
    private BallShooter ballShooter;
    private Vector3 initialBackboardPosition;
    private bool isActive = false;

    private void Start()
    {
        if (aiBackboardTarget != null)
            initialBackboardPosition = aiBackboardTarget.position;

        if (GameManager.Instance != null && GameManager.Instance.CurrentGameMode == GameMode.VsAI)
        {
            isActive = true;
            MoveToPosition();
            SpawnBall();
            StartCoroutine(AILoop());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        isActive = false;
    }

    private IEnumerator AILoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(shootDelay);

            if (!isActive || this == null) yield break;

            ExecuteAIShot();

            yield return new WaitForSeconds(resetDelay);

            if (!isActive || this == null) yield break;

            ResetAI();
        }
    }

    private void ExecuteAIShot()
    {
        if (currentBall == null || ballShooter == null) return;

        ShotPowerType powerType = GetShotTypeForDifficulty();

        basketDetector?.SetLastShotType(powerType);
        GameManager.Instance?.OnAIShotFired();
        ballShooter.ShootBall(basketTarget.position, aiBackboardTarget.position, powerType);
        AudioManager.Instance?.PlayBallLaunch();
    }

    private ShotPowerType GetShotTypeForDifficulty()
    {
        AIDifficulty difficulty = GameManager.Instance.CurrentDifficulty;
        bool bonusActive = GameManager.Instance.IsBonusActive;
        float roll = Random.value;

        if (bonusActive)
        {
            float chanceGood = 0f;
            float chanceNearGood = 0f;

            if (difficulty == AIDifficulty.Easy)
            {
                chanceGood = 0.10f;
                chanceNearGood = 0.22f;
            }
            else if (difficulty == AIDifficulty.Normal)
            {
                chanceGood = 0.40f;
                chanceNearGood = 0.58f;
            }
            else if (difficulty == AIDifficulty.Hard)
            {
                chanceGood = 0.82f;
                chanceNearGood = 0.88f;
            }

            if (roll < chanceGood) return ShotPowerType.Good;
            if (roll < chanceNearGood) return ShotPowerType.NearGood;
        }

        if (difficulty == AIDifficulty.Easy)
        {
            if (roll < 0.15f) return ShotPowerType.Perfect;
            if (roll < 0.30f) return ShotPowerType.NearPerfect;
            if (roll < 0.55f) return ShotPowerType.Normal;
            return ShotPowerType.Weak;
        }
        else if (difficulty == AIDifficulty.Normal)
        {
            if (roll < 0.40f) return ShotPowerType.Perfect;
            if (roll < 0.62f) return ShotPowerType.NearPerfect;
            if (roll < 0.82f) return ShotPowerType.Normal;
            return ShotPowerType.Weak;
        }
        else if (difficulty == AIDifficulty.Hard)
        {
            bool fireballActive = FireballManager.Instance != null && FireballManager.Instance.IsFireballActive;

            if (fireballActive)
            {
                if (roll < 0.70f) return ShotPowerType.Perfect;
                if (roll < 0.88f) return ShotPowerType.Good;
                if (roll < 0.97f) return ShotPowerType.NearPerfect;
                return ShotPowerType.Normal;
            }

            if (roll < 0.55f) return ShotPowerType.Perfect;
            if (roll < 0.73f) return ShotPowerType.Good;
            if (roll < 0.90f) return ShotPowerType.NearPerfect;
            return ShotPowerType.Normal;
        }

        return ShotPowerType.Normal;
    }

    private void ResetAI()
    {
        if (currentBall != null)
            Destroy(currentBall);

        MoveToPosition();
        SpawnBall();
    }

    private void MoveToPosition()
    {
        if (spawnManager == null || aiTransform == null) return;

        Transform nextPosition = GetPositionAvoidingPlayer();
        aiTransform.position = nextPosition.position;

        if (basketTarget != null)
        {
            Vector3 dir = basketTarget.position - aiTransform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
                aiTransform.rotation = Quaternion.LookRotation(dir);
        }

        UpdateBackboardTarget();
    }

    private Transform GetPositionAvoidingPlayer()
    {
        for (int i = 0; i < 5; i++)
        {
            Transform candidate = spawnManager.GetNextSpawnPosition();

            if (playerTransform == null)
                return candidate;

            float distance = Vector3.Distance(candidate.position, playerTransform.position);

            if (distance > 1.5f)
                return candidate;
        }

        return spawnManager.GetNextSpawnPosition();
    }

    private void UpdateBackboardTarget()
    {
        if (basketTarget == null || aiBackboardTarget == null || aiTransform == null) return;

        float lateralOffset = (aiTransform.position.x - basketTarget.position.x) * 0.3f;
        lateralOffset = Mathf.Clamp(lateralOffset, -0.3f, 0.3f);

        Vector3 targetPosition = initialBackboardPosition;
        targetPosition.x += lateralOffset;
        aiBackboardTarget.position = targetPosition;
    }

    private void SpawnBall()
    {
        if (currentBall != null)
            Destroy(currentBall);

        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);

        if (!currentBall.TryGetComponent(out ballShooter))
            ballShooter = currentBall.AddComponent<BallShooter>();

        if (currentBall.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (aiBackboardTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aiBackboardTarget.position, 0.25f);

            if (basketTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(aiBackboardTarget.position, basketTarget.position);
            }
        }

        if (ballSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(ballSpawnPoint.position, 0.1f);
        }
    }
}