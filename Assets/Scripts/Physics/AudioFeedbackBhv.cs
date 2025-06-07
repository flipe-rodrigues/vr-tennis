using UnityEngine;

public class AudioFeedbackBhv : FeedbackBhv
{
    // Public fields
    public AudioClip audioClip;

    protected override void Play(Collision collision)
    {
        Vector3 position = collision.GetContact(0).point;

        float relativeSpeed = collision.relativeVelocity.magnitude;

        float baseVolume = Mathf.Clamp(relativeSpeed * amplitudeModifier, 0, 1);

        this.PlayClipAtPoint(position, relativeSpeed, baseVolume);
    }

    protected override void Play()
    {
        Vector3 position = TennisManager.Instance.Ball.Position;

        float relativeSpeed = (TennisManager.Instance.Ball.LinearVelocity - TennisManager.Instance.Racket.LinearVelocity).magnitude;

        float baseVolume = Mathf.Clamp(relativeSpeed * amplitudeModifier, 0, 1);

        this.PlayClipAtPoint(position, relativeSpeed, baseVolume);
    }

    private void PlayClipAtPoint(Vector3 position, float relativeSpeed, float baseVolume)
    {
        if (audioClip == null)
        {
            return;
        }

        // Create temporary GameObject
        GameObject gameObject = new GameObject($"One Shot Audio ({audioClip.name})");
        gameObject.transform.position = position;

        // Add AudioSource component
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.dopplerLevel = 0.5f;

        // Add natural variation
        audioSource.pitch = Random.Range(0.9f, 1.1f) ;
        audioSource.volume = baseVolume * Random.Range(0.95f, 1.0f);

        // Play the audio clip
        audioSource.Play();

        // Destroy after clip finishes
        Destroy(gameObject, audioClip.length / audioSource.pitch);
        Object.Destroy(gameObject, audioClip.length * Time.timeScale);
    }
}
