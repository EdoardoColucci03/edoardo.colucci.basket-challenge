using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float maxSwipeDistance = 500f;
    [SerializeField] private float swipeThreshold = 10f;

    [Header("Mobile Adjustments")]
    [SerializeField] private float mobileDistanceMultiplier = 1.5f;

    [Header("Swipe Behavior")]
    [SerializeField] private bool cancelOnDownwardMovement = true;

    [Header("Auto-Shoot Settings")]
    [SerializeField] private bool enableAutoShoot = true;
    [SerializeField] private float autoShootDelay = 1f;

    private Vector2 startPosition;
    private Vector2 lastPosition;
    private bool isSwiping = false;
    private bool isHolding = false;
    private float swipeStartTime;
    private bool hasAutoShot = false;

    public float SwipePower { get; private set; }
    public bool IsSwipeActive => isSwiping;
    public bool ShouldAutoShoot { get; private set; }

    private void Update()
    {
        HandleInput();
        CheckAutoShoot();
    }

    private void HandleInput()
    {
        if (Application.isMobilePlatform)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    private void CheckAutoShoot()
    {
        if (!enableAutoShoot || !isHolding || hasAutoShot) return;

        float elapsedTime = Time.time - swipeStartTime;

        if (elapsedTime >= autoShootDelay)
        {
            hasAutoShot = true;
            ShouldAutoShoot = true;
            Debug.Log($"<color=yellow>[SwipeDetector] Auto-shoot triggered after {elapsedTime:F1}s</color>");
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            UpdateSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndSwipe(Input.mousePosition);
            isHolding = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartSwipe(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isSwiping)
                    {
                        UpdateSwipe(touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isSwiping || isHolding)
                    {
                        EndSwipe(touch.position);
                    }
                    isHolding = false;
                    break;
            }
        }
    }

    private void StartSwipe(Vector2 position)
    {
        startPosition = position;
        lastPosition = position;
        isSwiping = true;
        isHolding = true;
        swipeStartTime = Time.time;
        hasAutoShot = false;
        ShouldAutoShoot = false;
        SwipePower = 0f;
    }

    private void UpdateSwipe(Vector2 currentPosition)
    {
        if (cancelOnDownwardMovement && HasMovedDown(currentPosition))
        {
            CancelSwipe();
            return;
        }

        if (!IsValidSwipeDirection(currentPosition))
        {
            SwipePower = 0f;
            lastPosition = currentPosition;
            return;
        }

        float distance = CalculateSwipeDistance(currentPosition);
        float adjustedMaxDistance = GetAdjustedMaxDistance();

        SwipePower = Mathf.Clamp01(distance / adjustedMaxDistance);
        lastPosition = currentPosition;
    }

    private void EndSwipe(Vector2 endPosition)
    {
        if (!IsValidSwipeDirection(endPosition))
        {
            SwipePower = 0f;
            isSwiping = false;
            ShouldAutoShoot = false;
            return;
        }

        float distance = CalculateSwipeDistance(endPosition);
        float adjustedMaxDistance = GetAdjustedMaxDistance();

        if (distance >= minSwipeDistance)
        {
            SwipePower = Mathf.Clamp01(distance / adjustedMaxDistance);
        }
        else
        {
            SwipePower = 0f;
        }

        isSwiping = false;
        ShouldAutoShoot = false;
    }

    private bool IsValidSwipeDirection(Vector2 currentPosition)
    {
        float verticalDelta = currentPosition.y - startPosition.y;
        return verticalDelta > swipeThreshold;
    }

    private bool HasMovedDown(Vector2 currentPosition)
    {
        return currentPosition.y < lastPosition.y - swipeThreshold;
    }

    private float CalculateSwipeDistance(Vector2 currentPosition)
    {
        return Vector2.Distance(startPosition, currentPosition);
    }

    private float GetAdjustedMaxDistance()
    {
        if (Application.isMobilePlatform)
        {
            return maxSwipeDistance * mobileDistanceMultiplier;
        }
        return maxSwipeDistance;
    }

    public void CancelSwipe()
    {
        isSwiping = false;
        Debug.Log("<color=orange>[SwipeDetector] Swipe canceled - moved down (but still holding)</color>");
    }

    public void ResetSwipe()
    {
        SwipePower = 0f;
        isSwiping = false;
        isHolding = false;
        hasAutoShot = false;
        ShouldAutoShoot = false;
    }
}
