using UnityEngine;

public class DecalFeedbackBhv : FeedbackBhv
{
    // Public fields
    public DecalBhv decalPrefab;
    [Range(0, 100)]
    public int decalPoolSize = 10;
    [Range(0, 1)]
    public float alphaModifier = .05f;
    public Vector3 scaleModifier = Vector3.one * .005f;

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

        Vector3 relativeVelocity = -collision.relativeVelocity;
        Vector3 relativeLocalVelocity = this.Transform.InverseTransformDirection(relativeVelocity);
        Vector3 tangentialVelocity = Vector3.ProjectOnPlane(relativeVelocity, Vector3.up);
        Quaternion rotation = Quaternion.LookRotation(tangentialVelocity, Vector3.up);
        Vector3 deltaScale = relativeLocalVelocity.ElementWiseMultiplication(scaleModifier).Abs();

        float alpha = Mathf.Clamp(collision.relativeVelocity.magnitude * alphaModifier, 0, _maxAlpha);

        decal.Position = contact.point;
        decal.Rotation = rotation;
        decal.Scale = _defaultScale + deltaScale;
        decal.initialColor.a = alpha;
        
        decal.Active = true;

        decal.FadeAndReturnTo(_decalPool);
    }
}
