using Meta.WitAi;
using UnityEngine;

public class DecalFeedbackBhv : FeedbackBhv
{
    // Public fields
    public DecalBhv decalPrefab;
    [Range(0, 100)]
    public int decalPoolSize = 10;
    [Range(0, 1)]
    public float alphaModifier = .05f;
    [Range(0, 1)]
    public float scaleModifier = .005f;

    // Private fields
    private ObjectPool<DecalBhv> _decalPool;
    private Vector3 _defaultScale;
    private float _maxAlpha;

    protected override void Awake()
    {
        base.Awake();

        if (decalPrefab == null)
        {
            return;
        }

        _decalPool = new ObjectPool<DecalBhv>(decalPrefab, decalPoolSize);
    }

    private void Start()
    {
        if (decalPrefab == null)
        {
            return;
        }

        _defaultScale = decalPrefab.Scale;

        _maxAlpha = decalPrefab.initialColor.a;
    }

    protected override void Play(Collision collision)
    {
        if (decalPrefab == null)
        {
            return;
        }

        DecalBhv decal = _decalPool.Get(activate: false);

        ContactPoint contact = collision.GetContact(0);
        Quaternion rotation = Quaternion.LookRotation(collision.transform.forward, contact.normal);
        Vector3 scale = Vector3.ProjectOnPlane(TennisManager.Instance.Ball.LinearVelocity, contact.normal).Abs();
        float alpha = Mathf.Clamp(TennisManager.Instance.Ball.LinearVelocity.magnitude * alphaModifier, 0, _maxAlpha);

        decal.Position = contact.point;
        decal.Rotation = rotation;
        decal.Scale = _defaultScale + scale * scaleModifier;
        decal.initialColor.a = alpha;
        
        decal.Active = true;

        decal.FadeAndReturnTo(_decalPool);
    }
}
