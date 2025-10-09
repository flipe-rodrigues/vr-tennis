using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TargetBhv : CachedTransformBhv
{
    // Static fields
    public static event Action<Vector3, float> onTargetAcquiredV2;

    // Public properties
    public MeshRenderer MeshRenderer => _meshRenderer;

    // Public fields
    [ColorUsage(true, true)]
    public Color acquisitionColor;
    [Range(.01f, 5f)]
    public float acquisitionDelay = 0.1f;
    [Range(.01f, 5f)]
    public float resetDelay = 1f;
    public bool disableOnAcquisition = true;
    public UnityEvent<Vector3, float> onTargetAcquired = new UnityEvent<Vector3, float>();

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
        StartCoroutine(this.FadeToCoroutine(_defaultColor, acquisitionDelay));
    }

    private void SetColor(Color color)
    {
        _material.color = color;
        _light.color = color;
    }

    public void ColorLerp(float t)
    {
        this.SetColor(Color.Lerp(_defaultColor, acquisitionColor, t));
    }

    public void TryAcquireAt(Vector3 position, float intensity)
    {
        StopAllCoroutines();
        StartCoroutine(this.AcquisitionCoroutine(position, intensity));
    }

    public void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(this.FadeToCoroutine(_defaultColor, resetDelay));
    }

    private IEnumerator AcquisitionCoroutine(Vector3 position, float intensity)
    {
        yield return FadeToCoroutine(acquisitionColor, acquisitionDelay);
        this.AcquireAt(position, intensity);
        yield return FadeToCoroutine(disableOnAcquisition ? Color.clear : _defaultColor, resetDelay);
        this.Active = !disableOnAcquisition;
    }

    private void AcquireAt(Vector3 position, float intensity)
    {
        onTargetAcquired?.Invoke(position, intensity);
        onTargetAcquiredV2?.Invoke(position, intensity);
        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetAcquired);
        //TaskManager.Instance.StartNextTrial();
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
