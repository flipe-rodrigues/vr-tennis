using UnityEngine;

[SelectionBase]
public class RacketBhv : MonoBehaviour
{
    // Public properties
    public RacketMeshBhv Mesh => _mesh;
    public Vector3 Forward => _transform.forward;
    public Vector3 Position => _kalmanPosition;
    public Vector3 LinearVelocity => _kalmanLinearVelocity;
    public Vector3 AngularVelocity => _smoothAngularVelocity;

    // Public fields
    [Header("Temporal Smoothing Settings:")]
    [Range(0.001f, 1f)]
    public float smoothingTimeConstant = 0.01f;
    [Header("Kalman Filter Settings:")]
    [Range(0f, 1f)]
    public float alpha = 0.5f;
    [Range(0f, 1f)]
    public float beta = 0.05f;

    // Readonly fields
    [SerializeField, ReadOnly]
    private Vector3 _rawLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _rawAngularVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _smoothLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _smoothAngularVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _kalmanPosition;
    [SerializeField, ReadOnly]
    private Vector3 _kalmanLinearVelocity;

    // Private fields
    private OptitrackRigidBody _optitrackRigidbody;
    private Transform _transform;
    private RacketColliderBhv _collider;
    private RacketMeshBhv _mesh;
    private float _smoothingRate;

    private void OnValidate()
    {
        _smoothingRate = smoothingTimeConstant.TauToLambda(Time.fixedDeltaTime);
    }

    private void Awake()
    {
        _optitrackRigidbody = this.GetComponentInParent<OptitrackRigidBody>();
        _transform = this.GetComponent<Transform>();
        _collider = this.GetComponentInChildren<RacketColliderBhv>();
        _mesh = this.GetComponentInChildren<RacketMeshBhv>();
    }

    protected virtual void FixedUpdate()
    {
        this.UpdateLinearVelocity();
        this.UpdateAngularVelocity();
        this.ApplySmoothing();
        this.ApplyKalmanFilter();
    }

    private void UpdateLinearVelocity()
    {
        _rawLinearVelocity = (_optitrackRigidbody.CurrentPosition - _optitrackRigidbody.PreviousPosition) / Time.fixedDeltaTime;
    }

    private void UpdateAngularVelocity()
    {
        Quaternion deltaRawRotation = _optitrackRigidbody.CurrentRotation * Quaternion.Inverse(_optitrackRigidbody.PreviousRotation);
        deltaRawRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        _rawAngularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void ApplySmoothing()
    {
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, _rawLinearVelocity, _smoothingRate);
        _smoothAngularVelocity = Vector3.Lerp(_smoothAngularVelocity, _rawAngularVelocity, _smoothingRate);
    }

    private void ApplyKalmanFilter()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 predictedPosition = _kalmanPosition + _kalmanLinearVelocity * dt;
        Vector3 residualPosition = _optitrackRigidbody.CurrentPosition - predictedPosition;
        _kalmanPosition = predictedPosition + alpha * residualPosition;

        Vector3 predictedLinearVelocity = _kalmanLinearVelocity;
        _kalmanLinearVelocity = predictedLinearVelocity + beta * residualPosition / dt;
    }

    //private void OnDrawGizmos()
    //{
        //Gizmos.color = Color.cyan;
        //Gizmos.DrawLine(this.Position, this.Position + _collider.ContactNormal * 0.5f);
        //if (TennisManager.Instance.Ball != null)
        //{
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawLine(this.Position, TennisManager.Instance.Ball.Position);
        //}
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(this.Position, this.Position + this.Forward * .25f);
        //if (TennisManager.Instance.Ball != null)
        //{
        //    Gizmos.color = Color.gray;
        //    Gizmos.DrawLine(this.Position, this.Position + TennisManager.Instance.RelativeVelocity.normalized * 0.5f);
        //}
    //}
}
