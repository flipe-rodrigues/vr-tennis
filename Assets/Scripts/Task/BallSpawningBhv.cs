using UnityEngine;
using UnityEngine.Events;

public class BallSpawningBhv : CachedTransformBhv
{
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
    public float spawnInterval = 5f;
    public UnityEvent onBallSpawn = new UnityEvent();

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

        _ballPool = new ObjectPool<BallRigidbodyBhv>(ballPrefab, ballPoolSize);
    }

    private void Start()
    {
        // Should not be here.. but for now we record the metadata here
        if (TrackingManager.Instance.saveData)
        {
            this.RecordMetaData();
        }
    }

    // Should not be here.. but for now we record the metadata here
    private void RecordMetaData()
    {
        TrackingManager.Instance.RecordEvent($"ball-spawner-interval-{spawnInterval}");
        TrackingManager.Instance.RecordEvent($"ball-spawner-position-{this.Position}");
        TrackingManager.Instance.RecordEvent($"ball-spawner-rotation-{this.Rotation}");
        TrackingManager.Instance.RecordEvent($"ball-initial-velocity-{linearSpeed}");
        TrackingManager.Instance.RecordEvent($"ball-initial-topspin-{topSpin}");
        TrackingManager.Instance.RecordEvent($"ball-initial-sidespin-{sideSpin}");
    }

    private void Update()
    {
        if (Time.time % spawnInterval < Time.deltaTime)
        {
            this.SpawnBall();

            onBallSpawn.Invoke();
        }
    }

    private void SpawnBall()
    {
        if (ballPrefab == null)
        {
            return;
        }

        if (_currentBall != null)
        {
            _ballPool.Return(_currentBall, deactivate: false);
        }

        _currentBall = _ballPool.Get();

        _currentBall.Move(this.Position, this.Rotation);
        _currentBall.LinearVelocity = this.Forward * linearSpeed;
        _currentBall.AngularVelocity = this.Right * topSpin + this.Up * sideSpin;

        TennisManager.Instance.Ball = _currentBall;
    }
}