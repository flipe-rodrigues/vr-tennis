using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer))]
public class TargetBhv : CachedTransformBhv
{
    // Public properties
    public MeshRenderer MeshRenderer => _meshRenderer;

    // Public fields
    [ColorUsage(true, true)]
    public Color acquisitionColor;
    [Range(.1f, 5f)]
    public float glowDelay = 0.1f;
    [Range(.1f, 5f)]
    public float fadeDelay = 1f;
    public UnityEvent<Vector3, float> onTargetAcquired = new UnityEvent<Vector3, float>();

    // Readonly fields
    [SerializeField, ReadOnly]
    private bool _isDisplayingAcquisition;

    // Private fields
    private MeshRenderer _meshRenderer;
    private Material _material;
    private Color _defaultColor;

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = this.GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;
    }

    private void Start()
    {
        _defaultColor = _material.color;
    }

    protected void TryDisplayProgress(float t)
    {
        if (_isDisplayingAcquisition)
        {
            return;
        }

        _material.color = Color.Lerp(_defaultColor, acquisitionColor, t);
    }

    protected void TryAcquireAt(Vector3 position, float intensity)
    {
        if (_isDisplayingAcquisition)
        {
            return;
        }

        onTargetAcquired?.Invoke(position, intensity);

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetAcquired);

        StartCoroutine(this.AcquisitionDisplayCoroutine());
    }

    private IEnumerator AcquisitionDisplayCoroutine()
    {
        _isDisplayingAcquisition = true;

        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.fixedDeltaTime / glowDelay;

            _material.color = Color.Lerp(_defaultColor, acquisitionColor, lerp);

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        while (lerp > 0)
        {
            lerp -= Time.fixedDeltaTime / fadeDelay;

            _material.color = Color.Lerp(_defaultColor, acquisitionColor, lerp);

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _material.color = _defaultColor;

        _isDisplayingAcquisition = false;
    }
}
