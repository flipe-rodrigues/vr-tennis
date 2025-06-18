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
        if (!TennisManager.Instance.Ball.WasJustHit)
        {
            return;
        }

        _mesh.GlowAndFade();

        onTargetHit?.Invoke(TennisManager.Instance.Ball.LinearVelocity.magnitude);
    }
}
