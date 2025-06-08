using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv : CachedTransformBhv
{
    // Public properties
    public Collider Collider => _collider;

    // Public fields
    [Range(0, 1)]
    public float scaleModifier = 0.1f;
    public float smoothingTimeConstant = 0.1f;

    // Read only fields
    [SerializeField, ReadOnly]
    private float _smoothingRate;

    // Private fields
    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private Vector3 _defaultScale;
    private Vector3 _smoothLinearVelocity;
    private Vector3 _deltaScale;

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

        _collider = GetComponent<Collider>();

        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, TennisManager.Instance.Racket.LinearVelocity, _smoothingRate);

        _deltaScale = new Vector3(Mathf.Abs(_smoothLinearVelocity.y), Mathf.Abs(_smoothLinearVelocity.z), Mathf.Abs(_smoothLinearVelocity.x)) * scaleModifier;

        // Missing something related to angular velocity... need to export from blender with actual dimensitons

        this.Scale = _defaultScale + _deltaScale;
    }

    public void StartRefractoryPeriod()
    {
        StopAllCoroutines();

        StartCoroutine(this.RefractoryPeriodCoroutine());
    }

    private IEnumerator RefractoryPeriodCoroutine()
    {
        _collider.enabled = false;

        _meshRenderer.enabled = false;


        yield return new WaitForSeconds(TennisManager.Instance.Racket.refractoryPeriod);

        _collider.enabled = true;

        _meshRenderer.enabled = true;
    }
}
