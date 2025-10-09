using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TargetBhv : CachedTransformBhv
{
    // Static fields
    public static event Action<TargetBhv> onTargetHit;
    public static event Action<TargetBhv> onTargetExpired;

    // Public properties
    public MeshRenderer MeshRenderer => _meshRenderer;

    // Public fields
    [ColorUsage(true, true)]
    public Color hitColor = new Color(0f, 1f, .75f, 1f);
    [Range(.01f, 5f)]
    public float hitDelay = 0.1f;
    [Range(.01f, 5f)]
    public float resetDelay = 1f;
    [Min(0)]
    public float expirationDelay = 10;
    public bool disableOnHit = true;

    // Readonly fields
    [SerializeField, ReadOnly]
    private Timer _expirationTimer;

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

    protected virtual void Start()
    {
        _expirationTimer = new Timer(expirationDelay);
        _expirationTimer.Start();
    }

    private void LateUpdate()
    {
        if (_expirationTimer.IsExpired)
        {
            this.Expire();
            _expirationTimer.Stop();
        }
    }

    public void Restart()
    {
        this.SetColor(Color.clear);
        StartCoroutine(this.FadeToCoroutine(_defaultColor, hitDelay));
    }

    private void SetColor(Color color)
    {
        _material.color = color;
        _light.color = color.SetAlpha(1f);
    }

    public void ColorLerp(float t)
    {
        this.SetColor(Color.Lerp(_defaultColor, hitColor, t));
    }

    public void TryHit()
    {
        StopAllCoroutines();
        StartCoroutine(this.TryHitCoroutine());
    }

    public void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(this.FadeToCoroutine(_defaultColor, resetDelay));
    }

    public override void Deactivate()
    {
        this.SlowDeactivate();
    }

    public void SlowDeactivate()
    {
        StartCoroutine(this.SlowDeactivateCoroutine());
    }

    private IEnumerator TryHitCoroutine()
    {
        yield return FadeToCoroutine(hitColor, hitDelay);
        this.Hit();
        yield return FadeToCoroutine(disableOnHit ? Color.clear : _defaultColor, resetDelay);
        this.Active = !disableOnHit;
    }

    private IEnumerator SlowDeactivateCoroutine()
    {
        yield return FadeToCoroutine(Color.clear, resetDelay);
        this.Active = false;
    }

    private void Hit()
    {
        onTargetHit?.Invoke(this);

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetHit);
    }

    private void Expire()
    {
        onTargetExpired?.Invoke(this);

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetExpired);
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
