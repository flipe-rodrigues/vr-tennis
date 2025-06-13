using UnityEngine;

public class CachedTransformBhv : CachedGameObjectBhv
{
    // Public properties
    public Transform Transform => _transform == null ? GetComponent<Transform>() : _transform;
    public Vector3 Forward => this.Transform.forward;
    public Vector3 Right => this.Transform.right;
    public Vector3 Up => this.Transform.up;
    public Vector3 Position { get { return this.Transform.position; } set { this.Transform.position = value; } }
    public Quaternion Rotation { get { return this.Transform.rotation; } set { this.Transform.rotation = value; } }
    public Vector3 Scale { get { return this.Transform.localScale; } set { this.Transform.localScale = value; } }

    // Private fields
    private Transform _transform;

    // Read only fields
    [SerializeField, ReadOnly]
    private Vector3 _position;
    [SerializeField, ReadOnly]
    private Vector3 _rotation;
    [SerializeField, ReadOnly]
    private Vector3 _scale;

    private void OnValidate()
    {
        _position = this.Transform.position;
        _rotation = this.Transform.eulerAngles;
        _scale = this.Transform.localScale;
    }

    protected override void Awake()
    {
        base.Awake();

        _transform = this.Transform;
    }
}
