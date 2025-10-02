using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CourtTargetBhv : TargetBhv 
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<BallRigidbodyBhv>().WasJustHit)
        {
            return;
        }

        this.TryAcquireAt(TennisManager.Instance.Ball.Position, TennisManager.Instance.Ball.LinearVelocity.magnitude);

        if (other == TennisManager.Instance.Ball.Collider)
        {
            TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetEnter);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == TennisManager.Instance.Ball.Collider)
        {
            TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetExit);
        }
    }
}
