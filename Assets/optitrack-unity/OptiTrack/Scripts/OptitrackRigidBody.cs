using System;
using UnityEngine;

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

    // Private fields
    private Transform _transform;
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private Quaternion _currentRotation;
    private Quaternion _previousRotation;

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

    protected virtual void FixedUpdate()
    {
        if (streamingClient == null)
        {
            return;
        }

        this.UpdatePose();
    }

    private void UpdatePose()
    {
        OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(_rigidbodyId, networkCompensation);
        if (rbState == null)
        {
            return;
        }

        _previousPosition = _currentPosition;
        _previousRotation = _currentRotation;

        _currentPosition = rbState.Pose.Position;
        _currentRotation = rbState.Pose.Orientation;
    }

    protected virtual void LateUpdate()
    {
        _transform.SetPositionAndRotation(_currentPosition, _currentRotation);
    }
}