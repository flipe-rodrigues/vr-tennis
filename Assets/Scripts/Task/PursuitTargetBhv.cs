using UnityEngine;

public class PursuitTargetBhv : TargetBhv
{
    // Public fields
    [Range(0, 1)]
    public float maximumDistance = .5f;

    // Readonly fields
    [SerializeField, ReadOnly, Range(0, 1)]
    private float _normalizedDistance;

    // Private fields
    private ParticleSystem _particles;

    protected override void Awake()
    {
        base.Awake();

        _particles = this.GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(this.Position, TennisManager.Instance.Racket.Position);
        _normalizedDistance = Mathf.InverseLerp(0, maximumDistance, distance);
        base.ColorLerp(1.0f - _normalizedDistance);

        var main = _particles.main;
        main.startColor = base.MeshRenderer.material.color;
    }
}
