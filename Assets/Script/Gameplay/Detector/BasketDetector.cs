using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketDetector : MonoBehaviour
{
    [SerializeField] private float minVelocityY = -0.5f;

    private bool hasHitRim = false;
    private bool hasHitBackboard = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Rigidbody ballRb = other.GetComponent<Rigidbody>();

            if (ballRb != null && ballRb.velocity.y < minVelocityY)
            {
                DetermineShotType();
            }
        }
    }

    private void DetermineShotType()
    {
        string shotType;
        int points;

        if (!hasHitRim && !hasHitBackboard)
        {
            shotType = "PERFECT SHOT!";
            points = 3;
            GameManager.Instance?.OnPerfectShot();
        }
        else if (hasHitBackboard && !hasHitRim)
        {
            shotType = "BACKBOARD BASKET!";
            points = 2;
            GameManager.Instance?.OnNormalBasket();
        }
        else
        {
            shotType = "BASKET!";
            points = 2;
            GameManager.Instance?.OnNormalBasket();
        }

        Debug.Log($"<color=yellow>{shotType} +{points} points</color>");
        ResetShotState();
    }

    public void OnRimHit()
    {
        hasHitRim = true;
    }

    public void OnBackboardHit()
    {
        hasHitBackboard = true;
    }

    private void ResetShotState()
    {
        hasHitRim = false;
        hasHitBackboard = false;
    }
}