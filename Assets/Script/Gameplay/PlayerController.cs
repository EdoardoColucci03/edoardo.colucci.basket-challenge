using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public bool IsBallInFlight { get; private set; }

    [Header("Ball Reference")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("Player Position Management")]
    [SerializeField] private SpawnPositionManager spawnManager;
    [SerializeField] private Transform playerTransform;

    [Header("Basket Reference")]
    [SerializeField] private Transform basketTarget;
    [SerializeField] private Transform backboardTarget;
    [SerializeField] private BasketDetector basketDetector;

    [Header("Camera")]
    [SerializeField] private CameraFollow basketballCamera;

    [Header("Auto Reset Settings")]
    [SerializeField] private float autoResetDelay = 2f;

    [Header("UI")]
    [SerializeField] private PowerBarUI powerBarUI;
    [SerializeField] private SwipeInput swipeDetector;
    [SerializeField] private TrajectoryVisualizer trajectoryVisualizer;
    [SerializeField] private bool showtrajectoryInRealtime = true;

    private GameObject currentBall;
    private BallShooter ballShooter;
    private bool hasBall = true;
    private bool isAiming = false;
    private bool inputEnabled = true;
    private Coroutine autoResetCoroutine;
    private Vector3 initialBackboardPosition;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ValidateReferences();

        if (backboardTarget != null)
        {
            initialBackboardPosition = backboardTarget.position;
            //Debug.Log($"<color=cyan>[BackboardTarget] Initial position saved: {initialBackboardPosition}</color>");
        }

        MovePlayerToPosition();
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

        if (spawnManager == null)
            Debug.LogError($"[PlayerController] SpawnPositionManager not assigned!", this);

        if (playerTransform == null)
        {
            Debug.LogWarning($"[PlayerController] Player Transform not assigned!");
            playerTransform = transform;
        }

        if (basketTarget == null)
            Debug.LogError($"[PlayerController] Basket Target not assigned!", this);

        if (backboardTarget == null)
            Debug.LogError($"[PlayerController] Backboard Target not assigned!", this);

        if (basketballCamera == null)
            Debug.LogWarning($"[PlayerController] BasketballCamera not assigned!");

        if (powerBarUI == null)
            Debug.LogError($"[PlayerController] PowerBarUI not assigned!", this);

        if (swipeDetector == null)
            Debug.LogError($"[PlayerController] SwipeDetector not assigned!", this);

        if (trajectoryVisualizer == null)
            Debug.LogError($"[PlayerController] TrajectoryVisualizer not assigned!", this);

        if (basketDetector == null)
            Debug.LogWarning($"[PlayerController] BasketDetector not assigned!", this);
    }

    private void UpdateBallPosition()
    {
        if (hasBall && currentBall != null && ballSpawnPoint != null)
        {
            currentBall.transform.position = ballSpawnPoint.position;
            currentBall.transform.rotation = ballSpawnPoint.rotation;
        }
    }

    private void HandleAimingInput()
    {
        if (!inputEnabled) return;

        if (Input.GetMouseButtonDown(0) && hasBall && !isAiming && autoResetCoroutine == null)
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
        if (!inputEnabled) return;

        if (swipeDetector.IsSwipeActive && isAiming)
        {
            float power = swipeDetector.SwipePower;
            powerBarUI.UpdatePower(power);
            UpdateTrajectoryPreview(power);
        }
    }

    private void HandleAutoShoot()
    {
        if (!inputEnabled) return;

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
        if (ballSpawnPoint == null || !showtrajectoryInRealtime) return;

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

        if (finalPower <= 0f)
        {
            CancelShot();
            return;
        }

        ShotPowerType powerType = powerBarUI.GetShotPowerType(finalPower);

        hasBall = false;
        isAiming = false;

        IsBallInFlight = true;

        basketDetector?.SetLastShotType(powerType);

        GameManager.Instance?.OnShotFired();

        ballShooter.ShootBall(basketTarget.position, backboardTarget.position, powerType);

        if (basketballCamera != null)
        {
            basketballCamera.StartBallCam();
        }

        CleanupAfterShot();

        //Debug.Log($"<color=orange>[PlayerController] Shot from {playerTransform.position} | Type: {powerType} | Swipe: {finalPower:F2}</color>");
    }

    private void CancelShot()
    {
        isAiming = false;
        if (showtrajectoryInRealtime) trajectoryVisualizer.HideTrajectory();
    }

    private void CleanupAfterShot()
    {
        swipeDetector.ResetSwipe();
        powerBarUI.FreezeBar();
        if (showtrajectoryInRealtime) trajectoryVisualizer.HideTrajectory();
    }

    public void ScheduleAutoReset()
    {
        if (autoResetCoroutine == null)
        {
            autoResetCoroutine = StartCoroutine(AutoResetRoutine());
        }
    }

    private IEnumerator AutoResetRoutine()
    {
        yield return new WaitForSeconds(autoResetDelay);
        ResetShot();
    }

    public void ResetShot()
    {
        IsBallInFlight = false;

        if (autoResetCoroutine != null)
        {
            StopCoroutine(autoResetCoroutine);
            autoResetCoroutine = null;
        }

        if (basketballCamera != null)
        {
            basketballCamera.StopBallCam();
        }

        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        MovePlayerToPosition();
        SpawnBall();
        GameManager.Instance?.OnBallReady();

        isAiming = false;
        swipeDetector.ResetSwipe();
        powerBarUI.ResetBar();
        trajectoryVisualizer.HideTrajectory();

        //Debug.Log($"<color=orange>[PlayerController] Ball Reset - Player moved to new position</color>");
    }

    private void MovePlayerToPosition()
    {
        if (spawnManager == null || playerTransform == null) return;

        Transform nextPosition = spawnManager.GetNextSpawnPosition();

        playerTransform.position = nextPosition.position;

        if (basketTarget != null)
        {
            Vector3 directionToBasket = basketTarget.position - playerTransform.position;
            directionToBasket.y = 0;

            if (directionToBasket != Vector3.zero)
            {
                playerTransform.rotation = Quaternion.LookRotation(directionToBasket);
            }
        }

        UpdateBackboardTarget();

        if (basketballCamera != null)
        {
            basketballCamera.UpdateCameraPosition();
        }

        //Debug.Log($"<color=orange>[PlayerController] Player at {nextPosition.name} facing basket</color>");
    }

    private void UpdateBackboardTarget()
    {
        if (basketTarget == null || backboardTarget == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 basketPos = basketTarget.position;

        float lateralOffset = (playerPos.x - basketPos.x) * 0.3f;
        lateralOffset = Mathf.Clamp(lateralOffset, -0.3f, 0.3f);

        Vector3 targetPosition = initialBackboardPosition;
        targetPosition.x += lateralOffset;

        backboardTarget.position = targetPosition;

        //Debug.Log($"<color=cyan>[BackboardTarget] PlayerX: {playerPos.x:F2} | BasketX: {basketPos.x:F2} | Offset: {lateralOffset:F2} | New Pos: {targetPosition}</color>");
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

        if (basketballCamera != null)
        {
            basketballCamera.SetBall(currentBall.transform);
        }

        hasBall = true;
    }

    public void SetInputEnabled(bool value)
    {
        inputEnabled = value;
    }

    private void OnDrawGizmos()
    {
        if (basketTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(basketTarget.position, 0.2f);
            Gizmos.DrawLine(basketTarget.position, basketTarget.position + Vector3.up * 0.5f);
        }

        if (backboardTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(backboardTarget.position, 0.25f);

            if (basketTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(backboardTarget.position, basketTarget.position);
            }
        }

        if (ballSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(ballSpawnPoint.position, 0.1f);
        }
    }
}