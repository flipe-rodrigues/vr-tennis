using UnityEngine;

[SelectionBase]
public class RacketBhv : MonoBehaviour
{
    // Public properties
    public RacketMeshBhv Mesh => _mesh;
    public Vector3 Forward
    {
        get
        {
            switch (forwardPreprocessing)
            {
                case KinematicPreprocessingType.AlphaBetaFilter:
                    return _alphaBetaRotation * Vector3.forward;
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothRotation * Vector3.forward;
                case KinematicPreprocessingType.None:
                default:
                    return _rawRotation * Vector3.forward;
            }
        }
    }
    public Vector3 Position
    {
        get
        {
            switch (positionPreprocessing)
            {
                case KinematicPreprocessingType.AlphaBetaFilter:
                    return _alphaBetaPosition;
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothPosition;
                case KinematicPreprocessingType.None:
                default:
                    return _rawPosition;
            }
        }
    }
    public Quaternion Rotation 
    {
        get
        {
            switch (rotationPreprocessing)
            {
                case KinematicPreprocessingType.AlphaBetaFilter:
                    return _alphaBetaRotation;
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothRotation;
                case KinematicPreprocessingType.None:
                default:
                    return _rawRotation;
            }
        }
    }
    public Vector3 LinearVelocity
    {
        get
        {
            switch (linearVelocityPreprocessing)
            {
                case KinematicPreprocessingType.AlphaBetaFilter:
                    return _alphaBetaLinearVelocity;
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothLinearVelocity;
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
                case KinematicPreprocessingType.AlphaBetaFilter:
                    return _alphaBetaAngularVelocity;
                case KinematicPreprocessingType.ExponentialSmoothing:
                    return _smoothAngularVelocity;
                case KinematicPreprocessingType.None:
                default:
                    return _rawAngularVelocity;
            }
        }
    }

    // Public fields
    [Header("Motion Capture:")]
    public OptiTrackRigidbody optitrackRigidbody;
    [SerializeField, ReadOnly]
    private Vector3 _rawPosition;
    [SerializeField, ReadOnly]
    private Quaternion _rawRotation;
    [SerializeField, ReadOnly]
    private Vector3 _rawLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _rawAngularVelocity;

    [Header("Preprocessing Settings:")]
    public KinematicPreprocessingType forwardPreprocessing = KinematicPreprocessingType.None;
    public KinematicPreprocessingType positionPreprocessing = KinematicPreprocessingType.None;
    public KinematicPreprocessingType rotationPreprocessing = KinematicPreprocessingType.None;
    public KinematicPreprocessingType linearVelocityPreprocessing = KinematicPreprocessingType.None;
    public KinematicPreprocessingType angularVelocityPreprocessing = KinematicPreprocessingType.None;

    [Header("Exponential Smoothing:")]
    [Range(0.001f, .5f)]
    public float smoothingTimeConstant = 0.01f;
    [SerializeField, ReadOnly]
    private float _smoothingRate;
    [SerializeField, ReadOnly]
    private Vector3 _smoothPosition;
    [SerializeField, ReadOnly]
    private Quaternion _smoothRotation;
    [SerializeField, ReadOnly]
    private Vector3 _smoothLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _smoothAngularVelocity;

    [Header("Alpha-Beta Filter:")]
    [Range(0.001f, .5f)]
    public float intendedTimeConstant = 0.01f;
    [SerializeField, ReadOnly]
    private float _suggestedAlpha;
    [SerializeField, ReadOnly]
    private float _suggestedBeta;
    [Range(0f, 1f)]
    public float alpha = 0.5f;
    [Range(0f, 1f)]
    public float beta = 0.05f;
    [SerializeField, ReadOnly]
    private Vector3 _alphaBetaPosition;
    [SerializeField, ReadOnly]
    private Quaternion _alphaBetaRotation;
    [SerializeField, ReadOnly]
    private Vector3 _alphaBetaLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _alphaBetaAngularVelocity;

    // Private fields
    private Transform _transform;
    private RacketMeshBhv _mesh;
    private Vector3 _previousRawPosition;
    private Quaternion _previousRawRotation;

    private void OnValidate()
    {
        _smoothingRate = smoothingTimeConstant.TauToLambda(Time.fixedDeltaTime);
        intendedTimeConstant.AlphaBetaFromTau(Time.fixedDeltaTime, out _suggestedAlpha, out _suggestedBeta);
    }

    private void Awake()
    {
        _transform = this.GetComponent<Transform>();
        _mesh = this.GetComponentInChildren<RacketMeshBhv>();
    }

    private void Start()
    {
        this.OnValidate();

        this.GetRawTrackingData();
        _smoothPosition = _rawPosition;
        _smoothRotation = _rawRotation;
        _alphaBetaPosition = _rawPosition;
        _alphaBetaRotation = _rawRotation;
    }

    protected virtual void FixedUpdate()
    {
        this.GetRawTrackingData();
        this.UpdateLinearVelocity();
        this.UpdateAngularVelocity();
        this.ApplyExponentialSmoothing();
        this.ApplyAlphaBetaFilterPosition();
        this.ApplyAlphaBetaFilterRotation();
    }

    private void GetRawTrackingData()
    {
        _previousRawPosition = optitrackRigidbody.PreviousPosition;
        _previousRawRotation = optitrackRigidbody.PreviousRotation;
        _rawPosition = optitrackRigidbody.CurrentPosition;
        _rawRotation = optitrackRigidbody.CurrentRotation;
    }

    private void LateUpdate()
    {
        _transform.position = this.Position;
        _transform.rotation = this.Rotation;
    }

    private void UpdateLinearVelocity()
    {
        _rawLinearVelocity = (_rawPosition - _previousRawPosition) / Time.fixedDeltaTime;
    }

    private void UpdateAngularVelocity()
    {
        Quaternion deltaRotation = _rawRotation * Quaternion.Inverse(_previousRawRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        if (angleInDegrees > 180f)
        {
            angleInDegrees -= 360f;
        }
        _rawAngularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void ApplyExponentialSmoothing()
    {
        _smoothPosition = Vector3.Lerp(_smoothPosition, _rawPosition, _smoothingRate);
        _smoothRotation = Quaternion.Slerp(_smoothRotation, _rawRotation, _smoothingRate);
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, _rawLinearVelocity, _smoothingRate);
        _smoothAngularVelocity = Vector3.Lerp(_smoothAngularVelocity, _rawAngularVelocity, _smoothingRate);
    }

    private void ApplyAlphaBetaFilterPosition()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 predictedPosition = _alphaBetaPosition + _alphaBetaLinearVelocity * dt;
        Vector3 residualPosition = _rawPosition - predictedPosition;
        _alphaBetaPosition = predictedPosition + alpha * residualPosition;

        Vector3 predictedLinearVelocity = _alphaBetaLinearVelocity;
        _alphaBetaLinearVelocity = predictedLinearVelocity + beta * residualPosition / dt;
    }

    private void ApplyAlphaBetaFilterRotation()
    {
        float dt = Time.fixedDeltaTime;

        // Convert angular velocity to quaternion increment
        Vector3 angularDisplacement = _alphaBetaAngularVelocity * dt;
        float angle = angularDisplacement.magnitude;
        Vector3 axis = angularDisplacement / angle;

        // Predict rotation using angular velocity
        Quaternion predictedDeltaRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
        Quaternion predictedRotation = predictedDeltaRotation * _alphaBetaRotation;

        // Calculate residual rotation (difference between measured and predicted)
        Quaternion residualRotation = _rawRotation * Quaternion.Inverse(predictedRotation);
        residualRotation.ToAngleAxis(out float residualAngle, out Vector3 residualAxis);

        // Handle angle wrapping
        if (residualAngle > 180f)
        {
            residualAngle -= 360f;
        }

        // Convert residual to angular displacement vector
        Vector3 residualAngularDisplacement = residualAxis * (residualAngle * Mathf.Deg2Rad);

        // Update filtered rotation (apply alpha correction)
        float correctionAngle = residualAngularDisplacement.magnitude * alpha;
        Vector3 correctionAxis = residualAngularDisplacement.normalized;
        Quaternion correction = Quaternion.AngleAxis(correctionAngle * Mathf.Rad2Deg, correctionAxis);
        _alphaBetaRotation = correction * predictedRotation;

        // Update filtered angular velocity (apply beta correction)
        _alphaBetaAngularVelocity = _alphaBetaAngularVelocity + (beta * residualAngularDisplacement / dt);
    }
}
