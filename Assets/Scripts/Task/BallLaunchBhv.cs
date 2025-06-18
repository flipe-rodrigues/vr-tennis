using UnityEngine;
using System;

public class BallLaunchBhv : CachedTransformBhv
{
    // Static fields
    public static Action onBallLaunch;

    // Public fields
    public BallRigidbodyBhv ballPrefab;
    [Range(0, 100)]
    public int ballPoolSize = 10;
    [Range(0, 100)]
    public float linearSpeed = 10f;
    [Range(-500, 500)]
    public float topSpin = 0f;
    [Range(-500, 500)]
    public float sideSpin = 0f;

    // Private fields
    private ObjectPool<BallRigidbodyBhv> _ballPool;
    private BallRigidbodyBhv _currentBall;

    protected override void Awake()
    {
        base.Awake();

        if (ballPrefab == null)
        {
            return;
        }

        _ballPool = new ObjectPool<BallRigidbodyBhv>(ballPrefab, ballPoolSize, this.Position);
    }

    private void OnEnable()
    {
        TaskManager.onTrialStart += this.HandleTrialStart;
    }

    private void OnDisable()
    {
        TaskManager.onTrialStart -= this.HandleTrialStart;
    }

    private void HandleTrialStart()
    {
        if (ballPrefab == null)
        {
            return;
        }
    
        this.LaunchBall();
    }

    private void LaunchBall()
    {
        if (_currentBall != null)
        {
            _ballPool.Return(_currentBall, deactivate: false);
        }

        _currentBall = _ballPool.Get();
        _currentBall.Move(this.Position, this.Rotation);
        _currentBall.Restart();
        _currentBall.LinearVelocity = this.Forward * linearSpeed;
        _currentBall.AngularVelocity = this.Right * topSpin + this.Up * sideSpin;

        TennisManager.Instance.Ball = _currentBall;

        onBallLaunch?.Invoke();

        TrackingManager.Instance.RecordEvent(TaskEventType.BallSpawn);
    }
}