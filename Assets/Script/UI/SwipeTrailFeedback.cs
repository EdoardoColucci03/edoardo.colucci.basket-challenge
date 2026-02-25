using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeTrailFeedback : MonoBehaviour
{
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float distanceFromCamera = 1f;

    private bool wasActive = false;

    private void Update()
    {
        bool isActive = swipeInput != null && swipeInput.IsSwipeActive;

        if (isActive)
        {
            Vector2 screenPos = Application.isMobilePlatform && Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceFromCamera));
            transform.position = worldPos;

            if (!wasActive)
            {
                trail.Clear();
                trail.emitting = true;
            }
        }
        else
        {
            trail.emitting = false;
        }

        wasActive = isActive;
    }
}

