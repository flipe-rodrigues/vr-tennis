using UnityEngine;

public class CollisionTrackingBhv : MonoBehaviour
{
    // Public fields
    public TaskEventType collisionEnterEvent;
    public TaskEventType collisionExitEvent;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == TennisManager.Instance.Ball.Collider)
        {
            TrackingManager.Instance.RecordTaskEvent(collisionEnterEvent);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider == TennisManager.Instance.Ball.Collider)
        {
            TrackingManager.Instance.RecordTaskEvent(collisionExitEvent);
        }
    }
}
