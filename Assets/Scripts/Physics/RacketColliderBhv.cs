using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv : CachedTransformBhv
{
    // Public fields
    public Vector3 scaleModifier = Vector3.one;
    [Range(1, 10)]
    public float maxScaleFactor = 5f;
    [Min(0.001f)]
    public float smoothingTimeConstant = 0.1f;

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
    }

    private void Start()
    {
        _defaultScale = this.Scale;
    }

    protected override void Awake()
    {
        base.Awake();

        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        //_smoothLinearVelocity = TennisManager.Instance.Racket.SmoothLinearVelocity;
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, TennisManager.Instance.Racket.LinearVelocity, _smoothingRate);

        Vector3 localVelocity = transform.InverseTransformDirection(_smoothLinearVelocity);

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

        yield return new WaitForSecondsRealtime(TennisManager.Instance.Racket.refractoryPeriod);

        _meshRenderer.enabled = true;

        _collider.enabled = true;
    }
}
