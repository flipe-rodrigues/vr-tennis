using UnityEngine;

[System.Serializable]
public class Timer
{
    // Public properties
    public float ElapsedTime => Time.time - _startTime;
    public bool IsExpired => this.ElapsedTime >= _duration;
    public bool IsRunning => _isRunning;

    // Readonly fields
    [SerializeField, ReadOnly, Min(0)]
    private float _startTime;
    [SerializeField, ReadOnly, Min(0)]
    private float _duration;
    [SerializeField, ReadOnly]
    private bool _isRunning;

    public Timer(float duration)
    {
        _duration = duration;
        _isRunning = false;
    }
    
    public void Start()
    {
        _startTime = Time.time;
        _isRunning = true;
    }

    public void Stop()
    {
        _startTime = Time.time;
        _isRunning = false;
    }
}
