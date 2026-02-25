using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBasketDetector : MonoBehaviour
{
    [SerializeField] private float minVelocityY = -0.3f;
    [SerializeField] private ParticleSystem basketVFX;

    private bool hasScored = false;
    private ShotPowerType lastShotType;

    public void SetLastShotType(ShotPowerType powerType)
    {
        lastShotType = powerType;
        hasScored = false;
    }

    public ShotPowerType GetLastShotType() => lastShotType;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("BallAI")) return;
        if (hasScored) return;

        Rigidbody ballRb = other.GetComponent<Rigidbody>();
        if (ballRb != null && ballRb.velocity.y < minVelocityY)
        {
            hasScored = true;
            DetermineShotType();
        }
    }

    private void DetermineShotType()
    {
        if (lastShotType == ShotPowerType.Perfect)
        {
            GameManager.Instance?.OnAIBasket(3);
            AudioManager.Instance?.PlayPerfectShot();
            AudioManager.Instance?.PlayBallNet();
        }
        else if (lastShotType == ShotPowerType.Good)
        {
            GameManager.Instance?.OnAIBackboardBasket();
            if (GameManager.Instance != null && GameManager.Instance.IsBonusActive)
                AudioManager.Instance?.PlayBonusBasket();
            AudioManager.Instance?.PlayBallNet();
        }
        else
        {
            GameManager.Instance?.OnAIBasket(2);
            AudioManager.Instance?.PlayBallNet();
        }

        basketVFX?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        basketVFX?.Play();

        lastShotType = ShotPowerType.None;
        hasScored = false;
    }
}