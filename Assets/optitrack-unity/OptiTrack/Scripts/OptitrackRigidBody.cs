using UnityEngine;
using System.Collections;

public class OptiTrackRigidbody : MonoBehaviour
{
    // Public properties
    public Vector3 CurrentPosition => _currentPosition;
    public Vector3 PreviousPosition => _previousPosition;
    public Quaternion CurrentRotation => _currentRotation;
    public Quaternion PreviousRotation => _previousRotation;

    // Public / readonly fields
    public OptiTrackStreamingClient streamingClient;
    public OptiTrackRigidbodyLabel rigidbodyLabel;
    [SerializeField, ReadOnly]
    private int _rigidbodyId;
    public bool networkCompensation = true;

    // Private fields
    private Transform _transform;
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private Quaternion _currentRotation;
    private Quaternion _previousRotation;
    private WaitForSeconds _waitForTrackingInterval;
    private float _nextSampleTime;

    private void OnValidate()
    {
        _rigidbodyId = (int)rigidbodyLabel;
    }

    private void Awake()
    {
        _transform = this.GetComponent<Transform>();
    }

    private void Start()
    {
        if (streamingClient == null)
        {
            streamingClient = OptiTrackStreamingClient.FindDefaultClient();
            if (streamingClient == null)
            {
                return;
            }
        }

        streamingClient.RegisterRigidBody(this, _rigidbodyId);
        //_waitForTrackingInterval = new WaitForSeconds(streamingClient.TrackingInterval);
        //StartCoroutine(this.OptiTrackUpdateCoroutine());
        _nextSampleTime = Time.fixedTime;
    }

    private void FixedUpdate()
    {
        if (streamingClient == null)
        {
            return;
        }

        if (Time.fixedTime >= _nextSampleTime)
        {
            this.UpdateTrackingState();
            this.UpdateTransform();
            _nextSampleTime += streamingClient.TrackingInterval / 2f;
        }
    }

    private IEnumerator OptiTrackUpdateCoroutine()
    {
        while (streamingClient != null)
        {
            this.UpdateTrackingState();
            this.UpdateTransform();

            yield return _waitForTrackingInterval;
        }
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