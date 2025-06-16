using UnityEngine;
using System.Collections;

public class DecalBhv : CachedTransformBhv
{
    // Private properties
    private MeshRenderer MeshRenderer => _meshRenderer == null ? GetComponentInChildren<MeshRenderer>() : _meshRenderer;

    // Public fields
    public Color initialColor;
    private float lifetime = 3f;

    // Private fields
    private MeshRenderer _meshRenderer;
    private Material _material;

    private void OnValidate()
    {
        this.MeshRenderer.sharedMaterial.color = initialColor;
    }

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = this.MeshRenderer;

        _material = _meshRenderer.material;
    }

    public void FadeAndReturnTo(ObjectPool<DecalBhv> pool)
    {
        this.StartCoroutine(FadeAndReturnToCoroutine(pool));
    }

    private IEnumerator FadeAndReturnToCoroutine(ObjectPool<DecalBhv> pool)
    {
        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.fixedDeltaTime / lifetime;

            _material.color = Color.Lerp(initialColor, Color.clear, lerp);

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        pool.Return(this, deactivate: true);
    }
}
