using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RacketHitDebugger : CachedTransformBhv
{
    // Public fields
    public RacketHitFields field = RacketHitFields.HitVelocity;
    public Vector3 offset;
    public Color color;

    // Read only fields
    [SerializeField, ReadOnly]
    private Vector3 _fieldValue;

    // Private fields
    private LineRenderer _lineRenderer;
    private string _propertyName;

    private void OnValidate()
    {
        this.name = $"RacketHitDebugger ({field})";

        GetComponent<LineRenderer>().startColor = color;
        GetComponent<LineRenderer>().endColor = color;
    }

    protected override void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _propertyName = field.ToString();
    }

    private void Update()
    {
        if (_lineRenderer == null)
        {
            return;
        }

        _fieldValue = GetFieldValueFromRacket(field);

        _lineRenderer.SetPosition(0, this.Position + offset);
        _lineRenderer.SetPosition(1, this.Position + offset + _fieldValue);
    }

    private Vector3 GetFieldValueFromRacket(RacketHitFields selectedField)
    {
        var racket = TennisManager.Instance.Racket;

        PropertyInfo prop = racket.GetType().GetProperty(_propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(Vector3))
        {
            return (Vector3)prop.GetValue(racket);
        }

        FieldInfo field = racket.GetType().GetField(_propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(Vector3))
        {
            return (Vector3)field.GetValue(racket);
        }

        Debug.LogWarning($"Field or property {_propertyName} not found on Racket.");
        return Vector3.zero;
    }
}

public enum RacketHitFields
{
    HitVelocity,
    HitContactNormal,
}