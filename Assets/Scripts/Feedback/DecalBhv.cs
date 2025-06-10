using Meta.Net.NativeWebSocket;
using System.Collections;
using UnityEngine;

public class DecalBhv : CachedTransformBhv
{
    // Private properties
    private MeshRenderer MeshRenderer => _meshRenderer == null ? GetComponentInChildren<MeshRenderer>() : _meshRenderer;

    // Public fields
    public Color initialColor = Color.black;
    private float lifetime = 3f;

    // Private fields
    private MeshRenderer _meshRenderer;

    private void OnValidate()
    {
        this.MeshRenderer.sharedMaterial.color = initialColor;
    }

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = this.MeshRenderer;
    }

    private IEnumerator Start()
    {
        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.deltaTime / lifetime;

            _meshRenderer.material.color = Color.Lerp(initialColor, Color.clear, lerp);

            yield return new WaitForUpdate();
        }

        Destroy(this.gameObject);
    }
}
