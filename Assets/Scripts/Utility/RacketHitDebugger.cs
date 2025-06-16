using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RacketHitDebugger : MonoBehaviour
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

    private void OnValidate()
    {
        this.name = $"RacketHitDebugger ({field})";

        GetComponent<LineRenderer>().startColor = color;
        GetComponent<LineRenderer>().endColor = color;
    }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (_lineRenderer == null)
        {
            return;
        }

        _fieldValue = GetFieldValueFromRacket(field);

        _lineRenderer.SetPosition(0, offset);
        _lineRenderer.SetPosition(1, offset + _fieldValue);
    }

    private Vector3 GetFieldValueFromRacket(RacketHitFields selectedField)
    {
        var racket = TennisManager.Instance.Racket;
        string propertyName = selectedField.ToString(); // e.g., "HitVelocity", "HitContactNormal"

        PropertyInfo prop = racket.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(Vector3))
        {
            return (Vector3)prop.GetValue(racket);
        }

        FieldInfo field = racket.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(Vector3))
        {
            return (Vector3)field.GetValue(racket);
        }

        Debug.LogWarning($"Field or property {propertyName} not found on Racket.");
        return Vector3.zero;
    }
}

public enum RacketHitFields
{
    HitVelocity,
    HitContactNormal,
}