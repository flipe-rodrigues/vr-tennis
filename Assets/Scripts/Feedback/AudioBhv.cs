using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioBhv : CachedTransformBhv
{
    // Public properties
    public AudioSource Source => _audioSource;

    // Private fields
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayAndReturnTo(ObjectPool<AudioBhv> pool)
    {
        this.StartCoroutine(PlayAndReturnToCoroutine(pool));
    }

    private IEnumerator PlayAndReturnToCoroutine(ObjectPool<AudioBhv> pool)
    {
        _audioSource.Play();

        yield return new WaitForSeconds(_audioSource.clip.length);

        pool.Return(this, deactivate: true);
    }
}
