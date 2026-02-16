using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackboardDetector : MonoBehaviour
{
    [SerializeField] private BasketDetector basketDetector;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            basketDetector.OnBackboardHit();
            Debug.Log("Ball hit backboard");
        }
    }
}

