using UnityEngine;

public class CollisionTrackingBhv : MonoBehaviour
{
    // Public fields
    public TaskEventType collisionEnterEvent;
    public TaskEventType collisionExitEvent;

    private void OnCollisionEnter(Collision collision)
    {
        TrackingManager.Instance.RecordTaskEvent(collisionEnterEvent);
    }

    private void OnCollisionExit(Collision collision)
    {
        TrackingManager.Instance.RecordTaskEvent(collisionExitEvent);
    }
}
