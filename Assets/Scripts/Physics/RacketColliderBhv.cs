using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv : CachedTransformBhv
{
    // Public fields
    [Range(0, 1)]
    public float scaleModifier = 0.1f;
    public float lerpSpeed = 1f;

    // Private fields
    private Collider _collider;
    private Vector3 _smoothDeltaScale;

    protected override void Awake()
    {
        base.Awake();

        _collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        Vector3 v = TennisManager.Instance.Racket.SmoothLinearVelocity;

        Vector3 deltaScale = new Vector3(Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.x)) * scaleModifier;

        _smoothDeltaScale = Vector3.Lerp(_smoothDeltaScale, deltaScale, lerpSpeed * Time.fixedDeltaTime);

        // Missing something related to angular velocity... need to export from blender with actual dimensitons

        this.Scale = Vector3.one + _smoothDeltaScale;
    }
}
