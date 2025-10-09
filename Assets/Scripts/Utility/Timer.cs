using UnityEngine;

[System.Serializable]
public class Timer
{
    // Public properties
    public float ElapsedTime => Time.time - _startTime;
    public bool IsExpired => this.ElapsedTime >= duration && this.IsRunning;
    public bool IsRunning => _isRunning;

    // Public fields
    [Min(0)]
    public float duration;

    // Readonly fields
    [SerializeField, ReadOnly, Min(0)]
    private float _startTime;
    [SerializeField, ReadOnly]
    private bool _isRunning;

    public Timer(float duration)
    {
        this.duration = duration;
        _isRunning = false;
    }
    
    public void Start()
    {
        _startTime = Time.time;
        _isRunning = true;
    }

    public void Stop()
    {
        _isRunning = false;
    }
}
