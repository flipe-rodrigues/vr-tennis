using UnityEngine;
using UnityEngine.Events;

public class TargetBhv : CachedTransformBhv 
{
    // Public fields
    public UnityEvent<float> onTargetHit = new UnityEvent<float>();

    // Private fields
    private TargetMeshBhv _mesh;

    protected override void Awake()
    {
        base.Awake();

        _mesh = GetComponentInChildren<TargetMeshBhv>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<BallRigidbodyBhv>().WasJustHit)
        {
            return;
        }

        _mesh.GlowAndFade();

        onTargetHit?.Invoke(TennisManager.Instance.Ball.LinearVelocity.magnitude);

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
