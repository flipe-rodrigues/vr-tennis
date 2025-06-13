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

        float timer = 0f;

        while (timer < _audioSource.clip.length)
        {
            timer += Time.fixedDeltaTime;

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        pool.Return(this, deactivate: true);
    }
}
