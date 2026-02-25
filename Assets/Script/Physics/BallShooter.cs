using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [Header("Backboard Settings")]
    [SerializeField] private float highBackboardOffset = 0.5f;

    [Header("Physics Settings")]
    [SerializeField] private float arcHeight = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float spinSpeed = 5f;

    private Rigidbody ballRigidbody;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();
    }

    public void ShootBall(Vector3 basketTarget, Vector3 backboardTarget, ShotPowerType powerType)
    {
        if (ballRigidbody == null) return;

        Vector3 targetPosition = DetermineTarget(basketTarget, backboardTarget, powerType);
        Vector3 perfectVelocity = BasketballPhysics.CalculateVelocity(transform.position, targetPosition, arcHeight);
        Vector3 finalVelocity = ApplyPowerModifier(perfectVelocity, powerType);

        ballRigidbody.isKinematic = false;
        ballRigidbody.useGravity = true;
        ballRigidbody.velocity = finalVelocity;

        Vector3 spinDirection = Vector3.Cross(finalVelocity.normalized, Vector3.up).normalized;
        ballRigidbody.angularVelocity = spinDirection * spinSpeed;
    }

    public Vector3 GetProjectedVelocity(Vector3 startPos, Vector3 basketTarget, Vector3 backboardTarget, ShotPowerType powerType)
    {
        Vector3 targetPosition = DetermineTarget(basketTarget, backboardTarget, powerType);
        Vector3 perfectVelocity = BasketballPhysics.CalculateVelocity(startPos, targetPosition, arcHeight);
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
                powerMultiplier = Random.Range(0.92f, 1.08f);
                errorVector = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.10f, 0.10f),
                Random.Range(-0.12f, 0.12f)
                );
                break;

            case ShotPowerType.NearPerfect:
                powerMultiplier = Random.Range(0.96f, 1.04f);
                errorVector = new Vector3(
                Random.Range(-0.06f, 0.06f),
                Random.Range(-0.03f, 0.03f),
                Random.Range(-0.04f, 0.04f)
                );
                break;

            case ShotPowerType.Perfect:
                powerMultiplier = 1.0f;
                errorVector = Vector3.zero;
                break;

            case ShotPowerType.NearGood:
                powerMultiplier = Random.Range(0.98f, 1.05f);
                errorVector = new Vector3(
                Random.Range(-0.07f, 0.07f),
                Random.Range(-0.05f, 0.05f),
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

        return result;
    }

}