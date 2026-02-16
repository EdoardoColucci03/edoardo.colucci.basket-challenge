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
                Debug.Log("Ball entered basket");
            }
        }
    }

    public void OnRimHit()
    {
        hasHitRim = true;
    }

    public void OnBackboardHit()
    {
        hasHitBackboard = true;
    }

}