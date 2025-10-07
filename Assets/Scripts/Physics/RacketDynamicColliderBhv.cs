using UnityEngine;

public class RacketDynamicColliderBhv : RacketColliderBhv
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
    private Vector3 _defaultScale;
    private Vector3 _smoothLinearVelocity;

    protected override void OnValidate()
    {
        base.OnValidate();

        _smoothingRate = smoothingTimeConstant.TauToLambda();
    }

    protected override void Start()
    {
        base.Start();

        _defaultScale = this.Scale;
    }

    private void FixedUpdate()
    {
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, TennisManager.Instance.Racket.LinearVelocity, _smoothingRate);

        Vector3 localVelocity = this.Transform.InverseTransformDirection(_smoothLinearVelocity);

        Vector3 deltaScale = localVelocity.ElementWiseMultiplication(scaleModifier).Abs();

        this.Scale = (_defaultScale + deltaScale).ClampBetween(_defaultScale, _defaultScale * maxScaleFactor);
    }
}
