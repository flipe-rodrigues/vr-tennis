using Oculus.Haptics;
using UnityEngine;

public class RacketHitFeedbackBhv : MonoBehaviour
{
    // Public fields
    public AudioClip audioClip;
    public HapticClip hapticClip;
    [Range(0, 1)]
    public float audioAmplitudeModifier = 0.1f;
    [Range(0, 1)]
    public float hapticAmplitudeModifier = 0.1f;

    //private void OnEnable()
    //{
    //    TennisManager.Instance.Racket.OnHit.AddListener(this.PlayAudioClip);
    //    TennisManager.Instance.Racket.OnHit.AddListener(this.PlayHapticClip);
    //}

    //private void OnDisable()
    //{
    //    TennisManager.Instance.Racket.OnHit.RemoveListener(this.PlayAudioClip);
    //    TennisManager.Instance.Racket.OnHit.RemoveListener(this.PlayHapticClip);
    //}

    private void PlayAudioClip()
    {
        Vector3 position = TennisManager.Instance.Ball.Position;

        float relativeSpeed = (TennisManager.Instance.RelativeVelocity).magnitude;

        float baseVolume = Mathf.Clamp(relativeSpeed * audioAmplitudeModifier, 0, 1);

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
    }

    private void PlayHapticClip()
    {
        float relativeVelocity = (TennisManager.Instance.RelativeVelocity).magnitude;

        float amplitude = relativeVelocity * hapticAmplitudeModifier;

        HapticsManager.Instance.Clip = hapticClip;

        HapticsManager.Instance.Play(amplitude);
    }
}
