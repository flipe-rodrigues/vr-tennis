using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CachedMeshRendererBhv : CachedGameObjectBhv
{
    // Public properties
    public MeshRenderer Renderer => _meshRenderer;
    public Material Material => _material;

    // Private fields
    private MeshRenderer _meshRenderer;
    private Material _material;

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = this.GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;
    }
}
