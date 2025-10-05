using UnityEngine;

public class FollowTargetBhv : TargetBhv
{
    // Public fields
    [Range(0, 1)]
    public float maximumDistance = .5f;

    // Readonly fields
    [SerializeField, ReadOnly, Range(0, 1)]
    private float _normalizedDistance;

    private void Update()
    {
        float distance = Vector3.Distance(this.Position, TennisManager.Instance.Racket.Position);
        _normalizedDistance = Mathf.InverseLerp(0, maximumDistance, distance);
        base.ColorLerp(1.0f - _normalizedDistance);
    }
}
