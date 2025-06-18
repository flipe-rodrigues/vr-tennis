using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv : CachedTransformBhv
{
    // Private properties
    private MeshRenderer MeshRenderer => _meshRenderer == null ? GetComponent<MeshRenderer>() : _meshRenderer;

    // Public fields
    public Vector3 scaleModifier = Vector3.one;
    [Range(1, 10)]
    public float maxScaleFactor = 5f;
    [Min(0.001f)]
    public float smoothingTimeConstant = 0.1f;
    public bool displayAsMesh;

    // Read only fields
    [SerializeField, ReadOnly]
    private float _smoothingRate;

    // Private fields
    private MeshRenderer _meshRenderer;
    private Collider _collider;
    private Vector3 _defaultScale;
    private Vector3 _smoothLinearVelocity;

    private void OnValidate()
    {
        _smoothingRate = smoothingTimeConstant.TauToLambda();

        this.MeshRenderer.enabled = displayAsMesh;
    }

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        _defaultScale = this.Scale;

        _meshRenderer.enabled = displayAsMesh;
    }

    private void FixedUpdate()
    {
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, TennisManager.Instance.Racket.LinearVelocity, _smoothingRate);

        Vector3 localVelocity = this.Transform.InverseTransformDirection(_smoothLinearVelocity);

        Vector3 deltaScale = localVelocity.ElementWiseMultiplication(scaleModifier).Abs();

        this.Scale = (_defaultScale + deltaScale).ClampBetween(_defaultScale, _defaultScale * maxScaleFactor);
    }

    public void StartRefractoryPeriod()
    {
        StartCoroutine(this.RefractoryPeriodCoroutine());
    }

    private IEnumerator RefractoryPeriodCoroutine()
    {
        _meshRenderer.enabled = false;

        _collider.enabled = false;

        float timer = 0f;

        while (timer < TennisManager.Instance.Racket.refractoryPeriod)
        {
            timer += Time.fixedDeltaTime;

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _meshRenderer.enabled = displayAsMesh;

        _collider.enabled = true;
    }
}
