using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TOOL DI TEST - Non utilizzato nel gameplay di produzione.
/// 
/// Questo script permette di testare rapidamente tutti i tipi di tiro
/// utilizzando scorciatoie da tastiera (1-7).
/// 
/// Utilizzo:
/// - Bilanciamento parametri fisici dei tiri
/// - Debug rapido della fisica
/// 
/// Per il gameplay vero, vedere PlayerController.cs
/// </summary>
public class BallPhysicsTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform basketTarget;
    [SerializeField] private Transform backboardTarget;

    [Header("Trajectory Visualization")]
    [SerializeField] private TrajectoryVisualizer trajectoryVisualizer;
    [SerializeField] private bool showTrajectoryPreview = true;
    [SerializeField] private float previewDuration = 1f;

    private GameObject currentBall;
    private BallShooter ballShooter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestShot(ShotPowerType.Perfect, "PERFECT SHOT");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestShot(ShotPowerType.Good, "GOOD SHOT (Backboard)");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestShot(ShotPowerType.NearPerfect, "NEAR PERFECT");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestShot(ShotPowerType.Normal, "NORMAL");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestShot(ShotPowerType.Weak, "WEAK");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TestShot(ShotPowerType.TooStrong, "TOO STRONG");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TestShot(ShotPowerType.NearGood, "NEAR GOOD");
        }
    }

    private void TestShot(ShotPowerType powerType, string shotName)
    {
        Debug.Log($"<color=cyan>[PHYSICS TEST] {shotName}</color>");

        SpawnBall();

        if (showTrajectoryPreview && trajectoryVisualizer != null)
        {
            StartCoroutine(ShowTrajectoryAndShoot(powerType));
        }
        else
        {
            PerformShot(powerType);
        }
    }

    private IEnumerator ShowTrajectoryAndShoot(ShotPowerType powerType)
    {
        Vector3 projectedVelocity = ballShooter.GetProjectedVelocity(
            ballSpawnPoint.position,
            basketTarget.position,
            backboardTarget.position,
            powerType
        );

        trajectoryVisualizer.ShowTrajectory(
            ballSpawnPoint.position,
            projectedVelocity,
            powerType
        );

        yield return new WaitForSeconds(previewDuration);

        trajectoryVisualizer.HideTrajectory();
        PerformShot(powerType);
    }

    private void PerformShot(ShotPowerType powerType)
    {
        if (ballShooter == null)
        {
            Debug.LogError("<color=red>[PHYSICS TEST] BallShooter is null!</color>");
            return;
        }

        ballShooter.ShootBall(
            basketTarget.position,
            backboardTarget.position,
            powerType
        );
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
