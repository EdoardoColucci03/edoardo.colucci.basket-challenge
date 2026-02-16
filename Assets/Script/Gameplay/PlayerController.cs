using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ball Reference")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("Basket Reference")]
    [SerializeField] private Transform basketTarget;
    [SerializeField] private Transform backboardTarget;

    [Header("Auto Reset Settings")]
    [SerializeField] private float autoResetDelay = 2f;

    [Header("UI")]
    [SerializeField] private PowerBarUI powerBarUI;
    [SerializeField] private SwipeInput swipeDetector;
    [SerializeField] private TrajectoryVisualizer trajectoryVisualizer;

    private GameObject currentBall;
    private BallShooter ballShooter;
    private bool hasBall = true;
    private bool isAiming = false;
    private Coroutine autoResetCoroutine;

    public bool HasBall => hasBall;
    public bool IsAiming => isAiming;

    void Start()
    {
        ValidateReferences();
        SpawnBall();
    }

    void Update()
    {
        UpdateBallPosition();
        HandleAimingInput();
        HandleSwipeUpdate();
        HandleAutoShoot();
    }

    private void ValidateReferences()
    {
        if (ballPrefab == null)
            Debug.LogError($"[PlayerController] Ball Prefab not assigned!", this);

        if (ballSpawnPoint == null)
            Debug.LogError($"[PlayerController] Ball Spawn Point not assigned!", this);

        if (basketTarget == null)
            Debug.LogError($"[PlayerController] Basket Target not assigned!", this);

        if (backboardTarget == null)
            Debug.LogError($"[PlayerController] Backboard Target not assigned!", this);

        if (powerBarUI == null)
            Debug.LogError($"[PlayerController] PowerBarUI not assigned!", this);

        if (swipeDetector == null)
            Debug.LogError($"[PlayerController] SwipeDetector not assigned!", this);

        if (trajectoryVisualizer == null)
            Debug.LogError($"[PlayerController] TrajectoryVisualizer not assigned!", this);
    }

    private void UpdateBallPosition()
    {
        if (hasBall && currentBall != null)
        {
            currentBall.transform.position = ballSpawnPoint.position;
            currentBall.transform.rotation = ballSpawnPoint.rotation;
        }
    }

    private void HandleAimingInput()
    {
        if (Input.GetMouseButtonDown(0) && hasBall && autoResetCoroutine == null)
        {
            StartAiming();
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            ExecuteShot();
        }
    }

    private void HandleSwipeUpdate()
    {
        if (swipeDetector.IsSwipeActive && isAiming)
        {
            float power = swipeDetector.SwipePower;
            powerBarUI.UpdatePower(power);
            UpdateTrajectoryPreview(power);
        }
    }

    private void HandleAutoShoot()
    {
        if (swipeDetector.ShouldAutoShoot && isAiming)
        {
            ExecuteShot();
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        powerBarUI.UpdatePower(0);
    }

    private void UpdateTrajectoryPreview(float power)
    {
        ShotPowerType powerType = powerBarUI.GetShotPowerType(power);

        Vector3 projectedVelocity = ballShooter.GetProjectedVelocity(
            ballSpawnPoint.position,
            basketTarget.position,
            backboardTarget.position,
            powerType
        );

        trajectoryVisualizer.ShowTrajectory(ballSpawnPoint.position, projectedVelocity, powerType);
    }

    private void ExecuteShot()
    {
        if (!hasBall || currentBall == null || ballShooter == null)
        {
            CancelShot();
            return;
        }

        float finalPower = swipeDetector.SwipePower;
        ShotPowerType powerType = powerBarUI.GetShotPowerType(finalPower);

        hasBall = false;
        isAiming = false;

        ballShooter.ShootBall(basketTarget.position, backboardTarget.position, powerType);

        ScheduleAutoReset();
        CleanupAfterShot();

        Debug.Log($"<color=cyan>[PlayerController] Shot | Type: {powerType} | Swipe: {finalPower:F2}</color>");
    }

    private void CancelShot()
    {
        isAiming = false;
        trajectoryVisualizer.HideTrajectory();
    }

    private void CleanupAfterShot()
    {
        swipeDetector.ResetSwipe();
        powerBarUI.UpdatePower(0);
        trajectoryVisualizer.HideTrajectory();
    }

    public void ScheduleAutoReset()
    {
        if (autoResetCoroutine == null)
        {
            autoResetCoroutine = StartCoroutine(AutoResetRoutine());
        }
    }

    private System.Collections.IEnumerator AutoResetRoutine()
    {
        yield return new WaitForSeconds(autoResetDelay);
        ResetShot();
    }

    public void ResetShot()
    {
        if (autoResetCoroutine != null)
        {
            StopCoroutine(autoResetCoroutine);
            autoResetCoroutine = null;
        }

        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        SpawnBall();
        isAiming = false;
        swipeDetector.ResetSwipe();
        powerBarUI.UpdatePower(0);
        trajectoryVisualizer.HideTrajectory();

        Debug.Log($"<color=yellow>[{nameof(PlayerController)}] Ball Reset</color>");
    }

    private void SpawnBall()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);

        if (!currentBall.TryGetComponent(out ballShooter))
        {
            ballShooter = currentBall.AddComponent<BallShooter>();
        }

        if (currentBall.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        hasBall = true;
    }

    private void OnDrawGizmos()
    {
        if (basketTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(basketTarget.position, 0.15f);
            Gizmos.DrawLine(basketTarget.position, basketTarget.position + Vector3.up * 0.5f);
        }

        if (backboardTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(backboardTarget.position, 0.15f);
            Gizmos.DrawLine(backboardTarget.position, backboardTarget.position + Vector3.up * 0.5f);
        }
    }
}