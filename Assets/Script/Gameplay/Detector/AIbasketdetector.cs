using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBasketDetector : MonoBehaviour
{
    [SerializeField] private float minVelocityY = -0.3f;

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
            //Debug.Log("<color=red>[AI Basket] PERFECT +3</color>");
        }
        else if (lastShotType == ShotPowerType.Good)
        {
            GameManager.Instance?.OnAIBackboardBasket();
            //Debug.Log("<color=red>[AI Basket] BACKBOARD</color>");
        }
        else
        {
            GameManager.Instance?.OnAIBasket(2);
            //Debug.Log("<color=red>[AI Basket] +2</color>");
        }

        lastShotType = ShotPowerType.None;
        hasScored = false;
    }
}
