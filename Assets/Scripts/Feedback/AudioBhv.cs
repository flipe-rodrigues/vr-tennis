using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioBhv : CachedGameObjectBhv
{
    // Private fields
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = gameObject.GetComponent<AudioSource>();
    }
}
