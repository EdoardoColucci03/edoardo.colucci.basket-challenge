using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BasketballPhysics
{
    public static Vector3 CalculateVelocity(Vector3 start, Vector3 target, float height)
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
}