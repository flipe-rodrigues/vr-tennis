using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TargetMeshBhv : MonoBehaviour
{
    // Public fields
    [ColorUsage(true, true)]
    public Color glowColor;
    [Range(.1f, 10)]
    public float glowDelay = 1f;
    [Range(.1f, 10)]
    public float fadeDelay = 3f;

    // Private fields
    private MeshRenderer _meshRenderer;
    private Material _material;
    private Color _targetColor;
    private Color _initialColor;

    private void Awake()
    {
        _meshRenderer = this.GetComponent<MeshRenderer>();

        _material = _meshRenderer.material;
    }

    private void Start()
    {
        _initialColor = _material.color;
    }

    public void GlowAndFade()
    {
        this.StartCoroutine(GlowAndFadeCoroutine());
    }

    private IEnumerator GlowAndFadeCoroutine()
    {
        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.fixedDeltaTime / glowDelay;

            _targetColor = Color.Lerp(_initialColor, glowColor, lerp);

            _material.color = _targetColor;

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        while (lerp > 0)
        {
            lerp -= Time.fixedDeltaTime / fadeDelay;

            _targetColor = Color.Lerp(_initialColor, glowColor, lerp);

            _material.color = _targetColor;

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _material.color = _initialColor;
    }
}
