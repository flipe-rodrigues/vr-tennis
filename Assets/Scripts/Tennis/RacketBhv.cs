using UnityEngine;

[SelectionBase]
public class RacketBhv : MonoBehaviour
{
    // Public properties
    public RacketMeshBhv Mesh => _mesh;
    public Vector3 Forward => _transform.forward;
    public Vector3 Position
    {
        get
        {
            switch (positionPreprocessing)
            {
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothPosition;
                case KinematicPreprocessingType.KalmanFilter:
                    return _kalmanPosition;
                case KinematicPreprocessingType.None:
                default:
                    return optitrackRigidbody.CurrentPosition;
            }
        }
    }
    public Quaternion Rotation 
    {
        get
        {
            switch (rotationPreprocessing)
            {
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothRotation;
                case KinematicPreprocessingType.None:
                default:
                    return optitrackRigidbody.CurrentRotation;
            }
        }
    }
    public Vector3 LinearVelocity
    {
        get
        {
            switch (linearVelocityPreprocessing)
            {
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothLinearVelocity;
                case KinematicPreprocessingType.KalmanFilter:
                    return _kalmanLinearVelocity;
                case KinematicPreprocessingType.None:
                default:
                    return _rawLinearVelocity;
            }
        }
    }
    public Vector3 AngularVelocity 
    {
        get
        {
            switch (angularVelocityPreprocessing)
            {
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothAngularVelocity;
                case KinematicPreprocessingType.None:
                default:
                    return _rawAngularVelocity;
            }
        }
    }

    // Public fields
    [Header("Tracking Data Source:")]
    public OptitrackRigidBody optitrackRigidbody;
    [Header("Preprocessing Settings:")]
    public KinematicPreprocessingType positionPreprocessing = KinematicPreprocessingType.KalmanFilter;
    public KinematicPreprocessingType rotationPreprocessing = KinematicPreprocessingType.None;
    public KinematicPreprocessingType linearVelocityPreprocessing = KinematicPreprocessingType.KalmanFilter;
    public KinematicPreprocessingType angularVelocityPreprocessing = KinematicPreprocessingType.ExponentialSmoothing;
    [Header("Temporal Smoothing Settings:")]
    [Range(0.001f, .25f)]
    public float smoothingTimeConstant = 0.01f;
    [SerializeField, ReadOnly]
    private float _suggestedAlpha;
    [SerializeField, ReadOnly]
    private float _suggestedBeta;
    [Header("Kalman Filter Settings:")]
    [Range(0f, 1f)]
    public float alpha = 0.5f;
    [Range(0f, 1f)]
    public float beta = 0.05f;

    // Readonly fields
    [Header("Debugging:")]
    [SerializeField, ReadOnly]
    private Vector3 _rawLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _rawAngularVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _smoothPosition;
    [SerializeField, ReadOnly]
    private Quaternion _smoothRotation;
    [SerializeField, ReadOnly]
    private Vector3 _smoothLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _smoothAngularVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _kalmanPosition;
    [SerializeField, ReadOnly]
    private Vector3 _kalmanLinearVelocity;

    // Private fields
    private Transform _transform;
    private RacketColliderBhv _collider;
    private RacketMeshBhv _mesh;
    private float _smoothingRate;

    private void OnValidate()
    {
        _smoothingRate = smoothingTimeConstant.TauToLambda(Time.fixedDeltaTime);
        smoothingTimeConstant.AlphaBetaFromTau(Time.fixedDeltaTime, out _suggestedAlpha, out _suggestedBeta);
    }

    private void Awake()
    {
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

    private void LateUpdate()
    {
        _transform.position = this.Position;
        _transform.rotation = this.Rotation;
    }

    private void UpdateLinearVelocity()
    {
        _rawLinearVelocity = (optitrackRigidbody.CurrentPosition - optitrackRigidbody.PreviousPosition) / Time.fixedDeltaTime;
    }

    private void UpdateAngularVelocity()
    {
        Quaternion deltaRawRotation = optitrackRigidbody.CurrentRotation * Quaternion.Inverse(optitrackRigidbody.PreviousRotation);
        deltaRawRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        _rawAngularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void ApplySmoothing()
    {
        _smoothPosition = Vector3.Lerp(_smoothPosition, optitrackRigidbody.CurrentPosition, _smoothingRate);
        _smoothRotation = Quaternion.Slerp(_smoothRotation, optitrackRigidbody.CurrentRotation, _smoothingRate);
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, _rawLinearVelocity, _smoothingRate);
        _smoothAngularVelocity = Vector3.Lerp(_smoothAngularVelocity, _rawAngularVelocity, _smoothingRate);
    }

    private void ApplyKalmanFilter()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 predictedPosition = _kalmanPosition + _kalmanLinearVelocity * dt;
        Vector3 residualPosition = optitrackRigidbody.CurrentPosition - predictedPosition;
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
