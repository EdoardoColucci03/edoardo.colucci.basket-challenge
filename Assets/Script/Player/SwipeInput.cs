using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeInput : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeScreenPercent = 0.03f;
    [SerializeField] private float maxSwipeScreenPercent = 0.4f;
    [SerializeField] private float swipeThreshold = 10f;

    [Header("Swipe Behavior")]
    [SerializeField] private bool cancelOnDownwardMovement = true;

    [Header("Auto-Shoot Settings")]
    [SerializeField] private bool enableAutoShoot = true;
    [SerializeField] private float autoShootDelay = 1f;

    private float minSwipeDistance;
    private float maxSwipeDistance;

    private Vector2 startPosition;
    private Vector2 lastPosition;
    private bool isSwiping = false;
    private bool isHolding = false;
    private float swipeStartTime;
    private bool hasAutoShot = false;

    public float SwipePower { get; private set; }
    public bool IsSwipeActive => isSwiping;
    public bool ShouldAutoShoot { get; private set; }

    private void Start()
    {
        float screenHeight = Screen.height;
        minSwipeDistance = screenHeight * minSwipeScreenPercent;
        maxSwipeDistance = screenHeight * maxSwipeScreenPercent;

        Debug.Log($"[SwipeInput] Screen: {Screen.width}x{Screen.height} | DPI: {Screen.dpi} | MinSwipe: {minSwipeDistance:F0}px | MaxSwipe: {maxSwipeDistance:F0}px");
    }

    private void Update()
    {
        HandleInput();
        CheckAutoShoot();
    }

    private void HandleInput()
    {
        if (Application.isMobilePlatform)
            HandleTouchInput();
        else
            HandleMouseInput();
    }

    private void CheckAutoShoot()
    {
        if (!enableAutoShoot || !isHolding || hasAutoShot) return;

        float elapsedTime = Time.time - swipeStartTime;

        if (elapsedTime >= autoShootDelay)
        {
            hasAutoShot = true;
            ShouldAutoShoot = true;
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

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
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;
                StartSwipe(touch.position);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isSwiping)
                    UpdateSwipe(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isSwiping || isHolding)
                    EndSwipe(lastPosition);
                isHolding = false;
                break;
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
        SwipePower = Mathf.Clamp01(distance / maxSwipeDistance);
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

        SwipePower = distance >= minSwipeDistance
            ? Mathf.Clamp01(distance / maxSwipeDistance)
            : 0f;

        isSwiping = false;
        ShouldAutoShoot = false;
    }

    private bool IsValidSwipeDirection(Vector2 currentPosition)
    {
        return (currentPosition.y - startPosition.y) > swipeThreshold;
    }

    private bool HasMovedDown(Vector2 currentPosition)
    {
        return currentPosition.y < lastPosition.y - swipeThreshold;
    }

    private float CalculateSwipeDistance(Vector2 currentPosition)
    {
        return Vector2.Distance(startPosition, currentPosition);
    }

    public void CancelSwipe()
    {
        isSwiping = false;
        SwipePower = 0f;
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