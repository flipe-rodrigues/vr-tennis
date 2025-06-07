using UnityEngine;

public class DecalFeedbackBhv : FeedbackBhv
{
    // Public fields
    public DecalBhv decalPrefab;

    private void Start()
    {
        decalPrefab.transform.localScale = this.transform.localScale * amplitudeModifier;
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
        //decal.transform.localScale = (collision.relativeVelocity * amplitudeModifier).ClampBetween(Vector3.one * .02f, Vector3.one);
    }
}
