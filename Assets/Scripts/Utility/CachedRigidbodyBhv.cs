using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CachedRigidbodyBhv : CachedTransformBhv
{
    // Public properties
    public Vector3 LinearVelocity { get { return this.Rigidbody.linearVelocity; } set { this.Rigidbody.linearVelocity = value; } }
    public Vector3 AngularVelocity { get { return this.Rigidbody.angularVelocity; } set { this.Rigidbody.angularVelocity = value; } }

    // Private properties
    private Rigidbody Rigidbody => _rigidbody == null ? GetComponent<Rigidbody>() : _rigidbody;

    // Public fields
    public float maxAngularVelocity = 50;

    // Read only fields
    [SerializeField, ReadOnly]
    private Vector3 _linearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _angularVelocity;

    // Private fields
    private Rigidbody _rigidbody;
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;

    protected override void Awake()
    {
        base.Awake();

        _rigidbody = this.Rigidbody;
    }

    protected virtual void Start()
    {
        this.Rigidbody.maxAngularVelocity = maxAngularVelocity;

        _previousPosition = this.Position;
        _previousRotation = this.Rotation;
    }

    protected virtual void FixedUpdate()
    {
        if (this.Rigidbody.isKinematic)
        {
            this.UpdateKinematicLinearVelocity();
            this.UpdateKinematicAngularVelocity();
        }
        else
        {
            _linearVelocity = this.Rigidbody.linearVelocity;
            _angularVelocity = this.Rigidbody.angularVelocity;
        }
    }

    public void AddForce(Vector3 force)
    {
        this.Rigidbody.AddForce(force);
    }

    public void Move(Vector3 position, Quaternion rotation)
    {
        this.Rigidbody.Move(position, rotation);
    }

    private void UpdateKinematicLinearVelocity()
    {
        _linearVelocity = (this.Position - _previousPosition) / Time.fixedDeltaTime;

        _previousPosition = this.Position;
    }

    private void UpdateKinematicAngularVelocity()
    {
        Quaternion deltaRotation = this.Rotation * Quaternion.Inverse(_previousRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        _angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;

        _previousRotation = this.Rotation;
    }
}
