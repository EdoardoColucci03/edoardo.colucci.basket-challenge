using UnityEngine;

public class BackboardDetector : MonoBehaviour
{
    [SerializeField] private BasketDetector basketDetector;
    [SerializeField] private AIBasketDetector aiBasketDetector;
    [SerializeField] private Transform basketTarget;

    [Header("Rebound Settings")]
    [SerializeField] private float reboundArcHeight = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        bool isPlayerBall = collision.gameObject.CompareTag("Ball");
        bool isAIBall = collision.gameObject.CompareTag("BallAI");

        if (!isPlayerBall && !isAIBall) return;

        ShotPowerType lastShot = isPlayerBall ? basketDetector.GetLastShotType() : aiBasketDetector.GetLastShotType();

        if (isPlayerBall)
            basketDetector.OnBackboardHit();

        Debug.Log($"<color=cyan>[BackboardDetector] Hit | Tag: {collision.gameObject.tag} | ShotType: {lastShot}</color>");

        if (lastShot == ShotPowerType.Good)
        {
            Rigidbody ballRb = collision.gameObject.GetComponent<Rigidbody>();
            if (ballRb != null && basketTarget != null)
                ballRb.velocity = BasketballPhysics.CalculateVelocity(
                    collision.contacts[0].point, basketTarget.position, reboundArcHeight);
        }
    }
}