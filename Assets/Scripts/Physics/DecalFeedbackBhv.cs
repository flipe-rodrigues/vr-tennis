using UnityEngine;

public class DecalFeedbackBhv : FeedbackBhv
{
    // Public fields
    public DecalBhv decalPrefab;
    [Range(0, 1)]
    public float alphaModifier = .1f;
    [Range(0, 1)]
    public float scaleModifier = .1f;

    // Read only fields
    [SerializeField, ReadOnly]
    private Vector3 _defaultScale;
    [SerializeField, ReadOnly]
    private float _maxAlpha;

    private void Start()
    {
        _defaultScale = decalPrefab.transform.localScale;

        _maxAlpha = decalPrefab.initialColor.a;
    }

    protected override void Play(Collision collision)
    {
        if (decalPrefab == null)
        {
            return;
        }

        ContactPoint contact = collision.GetContact(0);

        Quaternion rotation = Quaternion.LookRotation(collision.transform.forward, contact.normal);

        DecalBhv decal = Instantiate(decalPrefab, contact.point, rotation);

        Vector3 scale = Vector3.ProjectOnPlane(TennisManager.Instance.Ball.LinearVelocity, contact.normal).Abs();

        decal.initialColor = new Color(
            0,
            0,
            0,
            Mathf.Clamp(TennisManager.Instance.RelativeVelocity.magnitude * alphaModifier, 0, _maxAlpha)
        );

        decal.Scale = _defaultScale + scale * scaleModifier;
    }
}
