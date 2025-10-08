using UnityEngine;

public class TrackedRigidbodyBhv : MonoBehaviour
{
    // Public properties
    public Vector3 Position => _position;
    public Vector3 LinearVelocity => _linearVelocity;
    public Quaternion Rotation => _rotation;
    public Vector3 AngularVelocity => _angularVelocity;

    // Public / readonly fields
    [Header("Optitrack Settings:")]
    public OptitrackStreamingClient streamingClient;
    public OptitrackRigidBodyLabel rigidBodyLabel;
    public bool networkCompensation = true;
    [SerializeField, ReadOnly]
    private int _rigidbodyId;

    [Header("Kalman Filter Settings:")]
    [Range(0f, 1f)] public float alpha = 0.4f;
    [Range(0f, 1f)] public float beta = 0.05f;
    [Range(0f, 1f)] public float alphaRot = 0.4f;
    [Range(0f, 1f)] public float betaRot = 0.05f;
    [SerializeField, ReadOnly]
    private Vector3 _position;
    [SerializeField, ReadOnly]
    private Vector3 _linearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _angularVelocity;
    [SerializeField, ReadOnly]
    private Quaternion _rotation;

    // Private fields
    private Transform _transform;
    private bool _hasInitialized = false;

    private void OnValidate()
    {
        _rigidbodyId = (int)rigidBodyLabel;
    }

    private void Awake()
    {
        _transform = this.GetComponent<Transform>();
    }

    private void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if (this.streamingClient == null)
        {
            this.streamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if (this.streamingClient == null)
            {
                Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(OptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
                this.enabled = false;
                return;
            }
        }

        this.streamingClient.RegisterRigidBody(this, _rigidbodyId);
    }

    private void FixedUpdate()
    {
        if (streamingClient == null)
        {
            return;
        }

        OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(_rigidbodyId, networkCompensation);

        if (rbState == null)
        {
            return;
        }

        float physicsDt = Time.fixedDeltaTime;
        Vector3 measPos = rbState.Pose.Position;
        Quaternion measRot = rbState.Pose.Orientation;

        if (!_hasInitialized)
        {
            _position = measPos;
            _rotation = measRot;
            _linearVelocity = Vector3.zero;
            _angularVelocity = Vector3.zero;
            _hasInitialized = true;
            return;
        }

        //
        // --- POSITION α-β FILTER ---
        //
        Vector3 predPos = _position + _linearVelocity * physicsDt;
        Vector3 residualPos = measPos - predPos;

        _position = predPos + alpha * residualPos;
        _linearVelocity = _linearVelocity + (beta / physicsDt) * residualPos;

        // --- ROTATION α-β FILTER ---
        //
        // Predict orientation forward by integrating angular velocity
        Quaternion deltaRot = Quaternion.Euler(_angularVelocity * Mathf.Rad2Deg * physicsDt);
        Quaternion predRot = deltaRot * _rotation;

        // Compute residual between predicted and measured rotations
        Quaternion residualRot = measRot * Quaternion.Inverse(predRot);
        residualRot.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;
        Vector3 residualAng = axis * Mathf.Deg2Rad * angleDeg;

        // Correct
        Quaternion correction = Quaternion.Euler(residualAng * Mathf.Rad2Deg * alphaRot);
        _rotation = correction * predRot;

        _angularVelocity += (betaRot / physicsDt) * residualAng;
    }
}
