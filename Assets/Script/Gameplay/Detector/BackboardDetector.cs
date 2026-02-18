using UnityEngine;

public class BackboardDetector : MonoBehaviour
{
    [SerializeField] private BasketDetector basketDetector;
    [SerializeField] private Transform basketTarget;

    [Header("Rebound Settings")]
    [SerializeField] private float reboundArcHeight = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ball")) return;

        basketDetector.OnBackboardHit();

        ShotPowerType lastShot = basketDetector.GetLastShotType();

        Debug.Log($"<color=cyan>[BackboardDetector] Ball hit backboard | Shot type: {lastShot}</color>");

        if (lastShot == ShotPowerType.Good)
        {
            Rigidbody ballRb = collision.gameObject.GetComponent<Rigidbody>();
            if (ballRb != null && basketTarget != null)
            {
                RedirectToBasket(ballRb, collision.contacts[0].point);
            }
        }
    }

    private void RedirectToBasket(Rigidbody ballRb, Vector3 contactPoint)
    {
        ballRb.velocity = BasketballPhysics.CalculateVelocity(contactPoint, basketTarget.position, reboundArcHeight);
    }
}