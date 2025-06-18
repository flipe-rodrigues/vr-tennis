using UnityEngine;

public class CollisionTrackingBhv : MonoBehaviour
{
    // Public fields
    public TaskEventType collisionEnterEvent;
    public TaskEventType collisionExitEvent;

    private void OnCollisionEnter(Collision collision)
    {
        TrackingManager.Instance.RecordEvent(collisionEnterEvent);
    }

    private void OnCollisionExit(Collision collision)
    {
        TrackingManager.Instance.RecordEvent(collisionExitEvent);
    }
    private void OnTriggerEnter(Collider collider)
    {
        TrackingManager.Instance.RecordEvent(collisionEnterEvent);
    }

    private void OnTriggerExit(Collider collider)
    {
        TrackingManager.Instance.RecordEvent(collisionExitEvent);
    }
}
