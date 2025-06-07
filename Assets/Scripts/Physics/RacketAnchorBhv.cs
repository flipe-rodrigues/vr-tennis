using UnityEngine;

public class RacketAnchorBhv : CachedTransformBhv
{
    // Public fields
    public RacketRigidbodyBhv anchoredRigidbody;

    // Read only fields
    [SerializeField, ReadOnly]
    private Vector3 _velocity;

    private void OnValidate()
    {
        if (anchoredRigidbody != null && anchoredRigidbody.anchorTransform != this)
        {
            anchoredRigidbody.anchorTransform = this;
        }
    }

    private void LateUpdate()
    {
        _velocity = TennisManager.Instance.Racket.SmoothLinearVelocity;

        Physics.SyncTransforms();
    }
}
