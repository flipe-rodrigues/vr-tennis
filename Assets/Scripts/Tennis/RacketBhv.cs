using UnityEngine;

[SelectionBase]
public class RacketBhv : MonoBehaviour
{
    // Public properties
    public RacketMeshBhv Mesh => _mesh;
    public Vector3 Position
    {
        get
        {
            switch (positionPreprocessing)
            {
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothPosition;
                case KinematicPreprocessingType.AlphaBeta:
                    return _alphaBetaPosition;
                case KinematicPreprocessingType.None:
                default:
                    return _currentPosition;
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
                    return _currentRotation;
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
                case KinematicPreprocessingType.AlphaBeta:
                    return _alphaBetaLinearVelocity;
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
    public KinematicPreprocessingType positionPreprocessing = KinematicPreprocessingType.AlphaBeta;
    public KinematicPreprocessingType rotationPreprocessing = KinematicPreprocessingType.None;
    public KinematicPreprocessingType linearVelocityPreprocessing = KinematicPreprocessingType.AlphaBeta;
    public KinematicPreprocessingType angularVelocityPreprocessing = KinematicPreprocessingType.ExponentialSmoothing;
    [Header("Temporal Smoothing Settings:")]
    [Range(0.001f, .25f)]
    public float smoothingTimeConstant = 0.01f;
    [SerializeField, ReadOnly]
    private float _suggestedAlpha;
    [SerializeField, ReadOnly]
    private float _suggestedBeta;
    [Header("Alpha-Beta Filter Settings:")]
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
    private Vector3 _alphaBetaPosition;
    [SerializeField, ReadOnly]
    private Vector3 _alphaBetaLinearVelocity;

    // Private fields
    private Transform _transform;
    private RacketColliderBhv _collider;
    private RacketMeshBhv _mesh;
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private Quaternion _currentRotation;
    private Quaternion _previousRotation;
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
        if (optitrackRigidbody == null)
        {
            _previousPosition = _currentPosition;
            _currentPosition = _transform.position;
            _previousRotation = _currentRotation;
            _currentRotation = _transform.rotation;
        }
        else
        {
            _previousPosition = optitrackRigidbody.PreviousPosition;
            _currentPosition = optitrackRigidbody.CurrentPosition;
            _previousRotation = optitrackRigidbody.PreviousRotation;
            _currentRotation = optitrackRigidbody.CurrentRotation;
        }
        
        this.UpdateLinearVelocity();
        this.UpdateAngularVelocity();
        this.ApplyExponentialSmoothing();
        this.ApplyAlphaBetaFilter();
    }

    private void LateUpdate()
    {
        _transform.position = this.Position;
        _transform.rotation = this.Rotation;
    }

    private void UpdateLinearVelocity()
    {
        _rawLinearVelocity = (_currentPosition - _previousPosition) / Time.fixedDeltaTime;
    }

    private void UpdateAngularVelocity()
    {
        Quaternion deltaRawRotation = _currentRotation * Quaternion.Inverse(_previousRotation);
        deltaRawRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        _rawAngularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void ApplyExponentialSmoothing()
    {
        _smoothPosition = Vector3.Lerp(_smoothPosition, _currentPosition, _smoothingRate);
        _smoothRotation = Quaternion.Slerp(_smoothRotation, _currentRotation, _smoothingRate);
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, _rawLinearVelocity, _smoothingRate);
        _smoothAngularVelocity = Vector3.Lerp(_smoothAngularVelocity, _rawAngularVelocity, _smoothingRate);
    }

    private void ApplyAlphaBetaFilter()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 predictedPosition = _alphaBetaPosition + _alphaBetaLinearVelocity * dt;
        Vector3 residualPosition = _currentPosition - predictedPosition;
        _alphaBetaPosition = predictedPosition + alpha * residualPosition;

        Vector3 predictedLinearVelocity = _alphaBetaLinearVelocity;
        _alphaBetaLinearVelocity = predictedLinearVelocity + beta * residualPosition / dt;
    }
}
