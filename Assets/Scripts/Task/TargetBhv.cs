using UnityEngine;

public class TargetBhv : CachedTransformBhv 
{
    // Private fields
    private TargetMeshBhv _mesh;

    protected override void Awake()
    {
        base.Awake();

        _mesh = GetComponentInChildren<TargetMeshBhv>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _mesh.GlowAndFade();
    }
}
