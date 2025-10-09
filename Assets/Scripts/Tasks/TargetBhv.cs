using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TargetBhv : CachedTransformBhv
{
    // Static fields
    public static event Action<TargetBhv> onTargetHit;

    // Public properties
    public MeshRenderer MeshRenderer => _meshRenderer;

    // Public fields
    [ColorUsage(true, true)]
    public Color hitColor = new Color(0f, 1f, .75f, 1f);
    [Range(.01f, 5f)]
    public float hitDelay = 0.1f;
    [Range(.01f, 5f)]
    public float resetDelay = 1f;
    public bool disableOnHit = true;

    // Private fields
    private MeshRenderer _meshRenderer;
    private Light _light;
    private Material _material;
    private Color _defaultColor;

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = this.GetComponent<MeshRenderer>();
        _light = this.GetComponent<Light>();
        _material = _meshRenderer.material;
        _defaultColor = _material.color;
    }

    public void Restart()
    {
        this.SetColor(Color.clear);
        StartCoroutine(this.FadeToCoroutine(_defaultColor, hitDelay));
    }

    private void SetColor(Color color)
    {
        _material.color = color;
        _light.color = color;
    }

    public void ColorLerp(float t)
    {
        this.SetColor(Color.Lerp(_defaultColor, hitColor, t));
    }

    public void TryHit()
    {
        StopAllCoroutines();
        StartCoroutine(this.HitCoroutine());
    }

    public void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(this.FadeToCoroutine(_defaultColor, resetDelay));
    }

    private IEnumerator HitCoroutine()
    {
        yield return FadeToCoroutine(hitColor, hitDelay);
        this.Hit();
        yield return FadeToCoroutine(disableOnHit ? Color.clear : _defaultColor, resetDelay);
        this.Active = !disableOnHit;
    }

    private void Hit()
    {
        onTargetHit?.Invoke(this);
        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetHit);
    }

    private IEnumerator FadeToCoroutine(Color finalColor, float duration)
    {
        float elapsedTime = 0;
        Color initialColor = _material.color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.fixedDeltaTime;
            this.SetColor(Color.Lerp(initialColor, finalColor, elapsedTime / duration));
            yield return ApplicationManager.waitForFixedUpdateInstance;
        }
        this.SetColor(finalColor);
    }
}
