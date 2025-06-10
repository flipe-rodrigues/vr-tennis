using UnityEngine;
using TMPro;

public class LinearVelocityToTextBhv : MonoBehaviour
{
    // Public fields
    public Vector3 offset;

    // Private fields
    private TextMeshProUGUI _text;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (_text != null)
        {
            float fps = 1.0f / Time.deltaTime;

            _text.text = $"{ TennisManager.Instance.Racket.SmoothLinearVelocity.magnitude:F2}";
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.SetPosition(0, offset);
            _lineRenderer.SetPosition(1, offset + TennisManager.Instance.Racket.HitVelocity);
        }
    }
}
