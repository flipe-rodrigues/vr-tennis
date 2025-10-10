using System;
using UnityEngine;
using static Meta.XR.MRUtilityKit.SceneDecorator.SceneDecorator;

public class BallLaunchBhv : CachedTransformBhv
{
    // Static fields
    public static Action<BallBhv> onBallLaunch;

    // Public fields
    public BallBhv ballPrefab;
    public TruncatedExponentialDistribution LaunchDelayDistribution = new TruncatedExponentialDistribution(1, 2, 4);
    [Range(0, 100)]
    public int ballPoolSize = 10;
    [Range(0, 100)]
    public float linearSpeed = 10f;
    [Range(-500, 500)]
    public float topSpin = 0f;
    [Range(-500, 500)]
    public float sideSpin = 0f;

    // Readonly fields
    [SerializeField, ReadOnly]
    private Timer _launchTimer = new Timer();

    // Private fields
    private ObjectPool<BallBhv> _ballPool;
    private BallBhv _currentBall;

    private void OnValidate()
    {
        LaunchDelayDistribution.UpdatePDF();
    }

    protected override void Awake()
    {
        base.Awake();

        if (ballPrefab == null)
        {
            return;
        }

        _ballPool = new ObjectPool<BallBhv>(ballPrefab, ballPoolSize, this.Position);
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
        this.StartPreLaunchDelay();
    }

    private void StartPreLaunchDelay()
    {
        _launchTimer.duration = LaunchDelayDistribution.Sample();
        _launchTimer.Start();
    }

    private void Update()
    {
        if (_launchTimer.IsExpired)
        {
            this.LaunchBall();
            _launchTimer.Stop();
        }
    }

    private void LaunchBall()
    {
        if (_currentBall != null)
        {
            _ballPool.Return(_currentBall, deactivate: false);
        }

        _currentBall = _ballPool.Get();
        _currentBall.Launch(this.Position, this.Rotation);
        _currentBall.LinearVelocity = this.Forward * linearSpeed;
        _currentBall.AngularVelocity = this.Right * topSpin + this.Up * sideSpin;
        onBallLaunch?.Invoke(_currentBall);

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.BallLaunch);
    }
}