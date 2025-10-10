using UnityEngine;
using System.Collections;

public class OptitrackRigidBody : MonoBehaviour
{
    // Public properties
    public Vector3 CurrentPosition => _currentPosition;
    public Vector3 PreviousPosition => _previousPosition;
    public Quaternion CurrentRotation => _currentRotation;
    public Quaternion PreviousRotation => _previousRotation;

    // Public / readonly fields
    public OptitrackStreamingClient streamingClient;
    public OptitrackRigidBodyLabel rigidBodyLabel;
    [SerializeField, ReadOnly]
    private int _rigidbodyId;
    public bool networkCompensation = true;
    public bool useDedicatedCoroutine = false;

    // Private fields
    private Transform _transform;
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private Quaternion _currentRotation;
    private Quaternion _previousRotation;
    private WaitForSeconds _waitForTrackingInterval;

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
        if (streamingClient == null)
        {
            streamingClient = OptitrackStreamingClient.FindDefaultClient();
            if (streamingClient == null)
            {
                return;
            }
        }

        streamingClient.RegisterRigidBody(this, _rigidbodyId);

        if (useDedicatedCoroutine)
        {
            _waitForTrackingInterval = new WaitForSeconds(streamingClient.TrackingInterval);
            StartCoroutine(this.OptiTrackUpdateCoroutine());
        }

    }

    private IEnumerator OptiTrackUpdateCoroutine()
    {
        while (true)
        {
            this.UpdateTrackingState();
            this.UpdateTransform();

            yield return _waitForTrackingInterval;
        }
    }

    private void FixedUpdate()
    {
        if (streamingClient == null || useDedicatedCoroutine)
        {
            return;
        }

        this.UpdateTrackingState();
    }

    private void LateUpdate()
    {
        if (streamingClient == null || useDedicatedCoroutine)
        {
            return;
        }

        this.UpdateTransform();
    }

    private void UpdateTrackingState()
    {
        OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(_rigidbodyId, networkCompensation);
        if (rbState != null)
        {
            _previousPosition = _currentPosition;
            _previousRotation = _currentRotation;
            _currentPosition = rbState.Pose.Position;
            _currentRotation = rbState.Pose.Orientation;
        }
    }

    private void UpdateTransform()
    {
        _transform.SetPositionAndRotation(_currentPosition, _currentRotation);
    }
}