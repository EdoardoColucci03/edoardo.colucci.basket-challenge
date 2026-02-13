using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketDetector : MonoBehaviour
{
    [SerializeField] private float minVelocityY = -1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Rigidbody ballRb = other.GetComponent<Rigidbody>();
            if (ballRb != null && ballRb.velocity.y < minVelocityY)
            {
                Debug.Log("Ball scored in the basket!");    
            }
        }
    }
}
