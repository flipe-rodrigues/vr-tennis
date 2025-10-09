using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CourtBhv : CachedTransformBhv 
{
    // Public properties
    public MeshRenderer MeshRenderer => _meshRenderer;

    // Private fields
    private MeshRenderer _meshRenderer;

    protected override void Awake() 
    { 
        base.Awake();

        _meshRenderer = this.GetComponent<MeshRenderer>();
    }
}
