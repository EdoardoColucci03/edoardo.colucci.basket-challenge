using UnityEngine;

public class BasketDetector : MonoBehaviour
{
    [SerializeField] private float minVelocityY = -0.5f;

    private bool hasHitRim = false;
    private bool hasHitBackboard = false;
    private bool hasScored = false;
    private ShotPowerType lastShotType;

    private CameraFollow cameraFollow;

    private void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    public void SetLastShotType(ShotPowerType powerType)
    {
        lastShotType = powerType;
        hasScored = false;
    }

    public ShotPowerType GetLastShotType()
    {
        return lastShotType;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (hasScored) return;

            Rigidbody ballRb = other.GetComponent<Rigidbody>();
            if (ballRb != null && ballRb.velocity.y < minVelocityY)
            {
                hasScored = true;
                DetermineShotType();
                cameraFollow?.StopBallCam();
            }
        }
    }

    private void DetermineShotType()
    {
        string shotType;
        int points;

        if (lastShotType == ShotPowerType.Perfect)
        {
            shotType = "PERFECT SHOT!";
            points = 3;
            GameManager.Instance?.OnPerfectShot();
            GameplayUI.Instance?.ShowScoreFlyer(points, shotType, Color.green);
        }
        else if (hasHitBackboard)
        {
            bool bonusActive = GameManager.Instance != null && GameManager.Instance.IsBonusActive;
            int bonusPoints = bonusActive ? GameManager.Instance.ActiveBonus.Points : 0;
            Color bonusColor = bonusActive ? GameManager.Instance.ActiveBonus.Color : new Color(1f, 0.5f, 0f);

            shotType = bonusActive ? "BACKBOARD BONUS!" : "BASKET!";
            points = 2 + bonusPoints;

            GameManager.Instance?.OnBackboardBasket();
            GameplayUI.Instance?.ShowScoreFlyer(points, shotType, bonusColor);
        }
        else
        {
            shotType = "BASKET!";
            points = 2;
            GameManager.Instance?.OnNormalBasket();
            GameplayUI.Instance?.ShowScoreFlyer(points, shotType, new Color(1f, 0.5f, 0f));
        }

        Debug.Log($"<color=green>{shotType} +{points} points</color>");
        ResetShotState();
    }

    public void OnRimHit()
    {
        hasHitRim = true;
        cameraFollow?.StopBallCam();
    }

    public void OnBackboardHit()
    {
        hasHitBackboard = true;
        cameraFollow?.StopBallCam();
    }

    private void ResetShotState()
    {
        hasHitRim = false;
        hasHitBackboard = false;
        lastShotType = ShotPowerType.None;
    }
}