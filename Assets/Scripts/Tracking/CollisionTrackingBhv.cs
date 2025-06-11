using UnityEngine;

public class CollisionTrackingBhv : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        string eventName = $"{this.name}CollisionEnter";

        TrackingManager.Instance.RecordEvent(eventName);
    }

    private void OnCollisionExit(Collision collision)
    {
        string eventName = $"{this.name}CollisionExit";

        TrackingManager.Instance.RecordEvent(eventName);
    }
}
