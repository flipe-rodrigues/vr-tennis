using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CachedRigidbodyBhvOLD : MonoBehaviour
{
    public Transform Transform => _transform == null ? GetComponent<Transform>() : _transform;
    public Rigidbody Rigidbody => _rigidbody == null ? GetComponent<Rigidbody>() : _rigidbody;
    public Vector3 Position => this.Rigidbody.position;
    public Vector3 LinearVelocity => this.Rigidbody.isKinematic ? this.GetKinematicLinearVelocity() : this.Rigidbody.linearVelocity;
    public Vector3 AngularVelocity => this.Rigidbody.angularVelocity;
    public Vector3 LastPosition => _lastPosition;
    public Vector3 LastLinearVelocity => _lastLinearVelocity;
    public Vector3 LastAngularVelocity => _lastAngularVelocity;
    public Quaternion Rotation => this.Rigidbody.rotation;

    public float linearVelocityTau = .1f;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    [SerializeField]
    private Vector3 _lastLinearVelocity;
    [SerializeField]
    private Vector3 _lastAngularVelocity;
    private float _lastUpdateTime;
    //protected List<Vector3> _positions = new List<Vector3>();

    protected virtual void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        _lastLinearVelocity = Vector3.zero;
        _lastPosition = _rigidbody.position;
        _lastRotation = _rigidbody.rotation;
    }

    protected virtual void FixedUpdate()
    {
        if (!_rigidbody.isKinematic)
        {
            _lastLinearVelocity = _rigidbody.linearVelocity;
            _lastAngularVelocity = _rigidbody.angularVelocity;
        }
        else
        {
            _lastLinearVelocity = this.LinearVelocity;

            Quaternion delta = _rigidbody.rotation * Quaternion.Inverse(_lastRotation);
            delta.ToAngleAxis(out float angleDeg, out Vector3 axis);
            if (angleDeg > 180f) {
                angleDeg -= 360f;          // keep sign consistent
            }
            float angleRad = angleDeg * Mathf.Deg2Rad;
            _lastAngularVelocity = axis * angleRad / Time.fixedDeltaTime;
        }
        _lastPosition = _rigidbody.position;
        _lastRotation = _rigidbody.rotation;
        _lastUpdateTime = Time.time;

        //_positions.Add(_rigidbody.position);
        //if (_positions.Count > 50)
        //{
        //    _positions.RemoveAt(0);
        //}
    }

    //private void OnDrawGizmos()
    //{
    //    for (int i = 1; i < _positions.Count; i++)
    //    {
    //        Gizmos.color = Color.Lerp(Color.clear, Color.white, (float)i / _positions.Count);

    //        Gizmos.DrawLine(_positions[i - 1], _positions[i]);
    //    }
    //}

    private Vector3 GetKinematicLinearVelocity()
    {
        float deltaTime = Time.time - _lastUpdateTime;

        if (deltaTime == 0f)
        {
            return Vector3.zero;
        }

        Vector3 rawVelocity = (this.Position - _lastPosition) / deltaTime;

        float lerp = 1f - Mathf.Exp(-linearVelocityTau * deltaTime);

        Vector3 smoothVelocity = Vector3.Lerp(_lastLinearVelocity, rawVelocity, lerp);

        return smoothVelocity;
    }
}
