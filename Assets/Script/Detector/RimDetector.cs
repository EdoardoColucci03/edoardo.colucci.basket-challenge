using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RimDetector : MonoBehaviour
{
    [SerializeField] private BasketDetector basketDetector;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            basketDetector.OnRimHit();
            Debug.Log("Ball hit rim");
        }
    }
}

