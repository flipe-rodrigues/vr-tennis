using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KinematicRigidbodyBhv : CachedRigidbodyBhv
{
    // Public properties
    public Vector3 SmoothLinearVelocity => _smoothLinearVelocity;
    public Vector3 SmoothForwardDirection => _smoothForwardDirection;

    // Public fields
    public float smoothingTau = 0.1f;

    // Read only fields
    [SerializeField, ReadOnly]
    private float _smoothingLambda;

    // Private fields
    private Vector3 _smoothLinearVelocity;
    private Vector3 _smoothForwardDirection;

    protected override void Start()
    {
        base.Start();

        _smoothLinearVelocity = Vector3.zero;

        _smoothingLambda = Mathf.Exp(-Time.fixedDeltaTime / smoothingTau);
    }

    private void UpdateSmoothLinearVelocity()
    {
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, this.LinearVelocity, 1f - _smoothingLambda);
    }

    private void UpdateSmoothForwardDirection()
    {
        _smoothForwardDirection = Vector3.Lerp(_smoothForwardDirection, this.Forward, 1f - _smoothingLambda);
    }
}
