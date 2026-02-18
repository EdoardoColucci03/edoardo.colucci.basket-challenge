using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform basket;
    [SerializeField] private Transform ball;

    [Header("Camera Positions")]
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float cameraDistance = 5f;

    [Header("Ball Cam Settings")]
    [SerializeField] private bool enableBallCam = true;
    [SerializeField] private float ballCamDuration = 3f;

    [SerializeField] private float ballCamDistance = 2.5f;

    [SerializeField] private float ballCamHeightOffset = 0.5f;

    [Header("Smooth Settings")]
    [SerializeField] private float positionSmoothSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isFollowingBall = false;
    private float ballCamTimer = 0f;
    private bool isFrozen = false;

    private Vector3 shotDirection;

    private void Start()
    {
        if (player != null)
            UpdateCameraPosition();
    }

    private void LateUpdate()
    {
        if (isFrozen) return;

        if (isFollowingBall && ball != null)
        {
            UpdateBallCam();
            ballCamTimer += Time.deltaTime;

            if (ballCamTimer >= ballCamDuration)
                StopBallCam();
        }
        else if (player != null)
        {
            UpdatePlayerCam();
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }

    private void UpdatePlayerCam()
    {
        Vector3 directionToBasket = (basket.position - player.position).normalized;
        directionToBasket.y = 0;

        Vector3 behindPlayer = player.position - directionToBasket * cameraDistance;
        behindPlayer.y = player.position.y + cameraHeight;

        targetPosition = behindPlayer;

        Vector3 lookTarget = basket.position;
        lookTarget.y = player.position.y + 1f;

        targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);
    }

    private void UpdateBallCam()
    {
        if (basket == null) return;

        Vector3 cameraPos = ball.position - shotDirection * ballCamDistance + Vector3.up * ballCamHeightOffset;

        targetPosition = cameraPos;

        Vector3 lookTarget = basket.position;
        targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);
    }

    public void StartBallCam()
    {
        if (!enableBallCam || ball == null) return;

        if (basket != null && ball != null)
        {
            shotDirection = (basket.position - ball.position);
            shotDirection.y = 0;
            shotDirection.Normalize();
        }

        isFrozen = false;
        isFollowingBall = true;
        ballCamTimer = 0f;
    }

    public void StopBallCam()
    {
        isFollowingBall = false;
        isFrozen = true;
        ballCamTimer = 0f;

        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    public void UpdateCameraPosition()
    {
        if (player == null || basket == null) return;

        isFrozen = false;
        UpdatePlayerCam();
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    public void SetBall(Transform newBall)
    {
        ball = newBall;
    }
}