using Oculus.Haptics;
using UnityEngine;

public class HapticsManager : Singleton<HapticsManager>
{
    // Read only fields
    [SerializeField, ReadOnly]
    private HapticSource _hapticSource;

    private void OnValidate()
    {
        _hapticSource = FindFirstObjectByType<HapticSource>();
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }

    public void Play(float amplitude)
    {
        if (_hapticSource == null)
        {
            return;
        }

        _hapticSource.amplitude = amplitude;

        _hapticSource.Play();
    }
}
