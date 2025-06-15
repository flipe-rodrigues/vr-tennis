using Oculus.Interaction;
using System.Collections;
using UnityEngine;

public class AudioFeedbackBhv : FeedbackBhv
{
    // Public fields
    public AudioBhv audioPrefab;
    [Range(0, 100)]
    public int audioPoolSize = 10;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volumeModifier = .1f;
    [Range(0, 1)]
    public float volumeVariationRange = .1f;
    [Range(0, 1)]
    public float pitchVariationRange = .2f;

    // Private fields
    private ObjectPool<AudioBhv> _audioPool;

    protected override void Awake()
    {
        base.Awake();

        if (audioClip == null)
        {
            return;
        }

        _audioPool = new ObjectPool<AudioBhv>(audioPrefab, audioPoolSize);
    }

    public override void Play(float relativeSpeed)
    {
        Vector3 position = TennisManager.Instance.Ball.Position;
        float baseVolume = Mathf.Clamp(relativeSpeed * volumeModifier, 0, 1);

        this.PlayClipAtPoint(position, relativeSpeed, baseVolume);
    }

    protected override void Play(Collision collision)
    {
        Vector3 position = collision.GetContact(0).point;
        float relativeSpeed = collision.relativeVelocity.magnitude;
        float baseVolume = Mathf.Clamp(relativeSpeed * volumeModifier, 0, 1);

        this.PlayClipAtPoint(position, relativeSpeed, baseVolume);
    }

    private void PlayClipAtPoint(Vector3 position, float relativeSpeed, float baseVolume)
    {
        if (audioClip == null)
        {
            return;
        }

        AudioBhv audio = _audioPool.Get(activate: true);

        audio.Position = position;

        audio.Source.clip = audioClip;
        audio.Source.pitch = Random.Range(1 - pitchVariationRange / 2f, 1 + pitchVariationRange / 2f);
        audio.Source.volume = baseVolume * Random.Range(1 - volumeVariationRange / 2f, 1 + volumeVariationRange / 2f);

        audio.PlayAndReturnTo(_audioPool);
    }
}
