using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [Header("Backboard Settings")]
    [SerializeField] private float highBackboardOffset = 0.5f;

    [Header("Physics Settings")]
    [SerializeField] private float arcHeight = 1.5f;

    private Rigidbody ballRigidbody;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();
    }

    public void ShootBall(Vector3 basketTarget, Vector3 backboardTarget, ShotPowerType powerType)
    {
        if (ballRigidbody == null) return;

        Vector3 targetPosition = DetermineTarget(basketTarget, backboardTarget, powerType);
        Vector3 perfectVelocity = CalculateVelocity(transform.position, targetPosition, arcHeight);
        Vector3 finalVelocity = ApplyPowerModifier(perfectVelocity, powerType);

        ballRigidbody.isKinematic = false;
        ballRigidbody.useGravity = true;
        ballRigidbody.velocity = finalVelocity;
    }

    public Vector3 GetProjectedVelocity(Vector3 startPos, Vector3 basketTarget, Vector3 backboardTarget, ShotPowerType powerType)
    {
        Vector3 targetPosition = DetermineTarget(basketTarget, backboardTarget, powerType);
        Vector3 perfectVelocity = CalculateVelocity(startPos, targetPosition, arcHeight);
        return ApplyPowerModifier(perfectVelocity, powerType);
    }

    private Vector3 DetermineTarget(Vector3 basketTarget, Vector3 backboardTarget, ShotPowerType powerType)
    {
        switch (powerType)
        {
            case ShotPowerType.Good:
                return backboardTarget;

            case ShotPowerType.NearGood:
                return backboardTarget;

            case ShotPowerType.TooStrong:
                return backboardTarget + Vector3.up * highBackboardOffset;

            default:
                return basketTarget;
        }
    }

    private Vector3 CalculateVelocity(Vector3 start, Vector3 target, float height)
    {
        Vector3 displacement = target - start;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float peakHeight = Mathf.Max(start.y, target.y) + height;
        float heightToReach = peakHeight - start.y;
        float heightToDrop = peakHeight - target.y;

        float timeToReach = Mathf.Sqrt(2f * heightToReach / gravity);
        float timeToDrop = Mathf.Sqrt(2f * heightToDrop / gravity);
        float totalTime = timeToReach + timeToDrop;

        float velocityY = Mathf.Sqrt(2f * gravity * heightToReach);
        Vector3 horizontalDirection = horizontalDisplacement.normalized;
        float velocityHorizontal = horizontalDistance / totalTime;

        return horizontalDirection * velocityHorizontal + Vector3.up * velocityY;
    }

    private Vector3 ApplyPowerModifier(Vector3 perfectVelocity, ShotPowerType powerType)
    {
        float powerMultiplier = 1f;
        Vector3 errorVector = Vector3.zero;

        switch (powerType)
        {
            case ShotPowerType.Weak:
                powerMultiplier = Random.Range(0.65f, 0.80f);
                errorVector = new Vector3(
                    Random.Range(-0.4f, 0.4f),
                    Random.Range(-0.3f, -0.1f),
                    Random.Range(-0.3f, 0.3f)
                );
                break;

            case ShotPowerType.Normal:
                powerMultiplier = Random.Range(0.88f, 1.12f);
                errorVector = new Vector3(
                    Random.Range(-0.25f, 0.25f),
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(-0.2f, 0.2f)
                );
                break;

            case ShotPowerType.NearPerfect:
                powerMultiplier = Random.Range(0.96f, 1.04f);
                errorVector = new Vector3(
                    Random.Range(-0.08f, 0.08f),
                    Random.Range(-0.05f, 0.05f),
                    Random.Range(-0.06f, 0.06f)
                );
                break;

            case ShotPowerType.Perfect:
                powerMultiplier = 1.0f;
                errorVector = Vector3.zero;
                break;

            case ShotPowerType.NearGood:
                powerMultiplier = Random.Range(0.98f, 1.05f);
                errorVector = new Vector3(
                    Random.Range(-0.10f, 0.10f),
                    Random.Range(-0.08f, 0.08f),
                    0f
                );
                break;

            case ShotPowerType.Good:
                powerMultiplier = 1.0f;
                errorVector = Vector3.zero;
                break;

            case ShotPowerType.TooStrong:
                powerMultiplier = 1.0f;
                errorVector = new Vector3(
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(0.0f, 0.2f),
                    0f
                );
                break;
        }

        Vector3 result = perfectVelocity * powerMultiplier + errorVector;

        if (result.z < 0)
        {
            result.z = 0f;
        }

        return result;
    }

}
