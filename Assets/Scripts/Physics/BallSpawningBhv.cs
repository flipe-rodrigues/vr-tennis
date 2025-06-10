using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallSpawningBhv : CachedTransformBhv
{
    // Public properties
    public Vector3 InitialVelocity => this.Forward * linearSpeed;

    // Public fields
    public GameObject ballPrefab;
    public TrackingBhv ballTracker;
    [Range(0, 100)]
    public int ballPoolSize = 10;
    [Range(0, 100)]
    public float linearSpeed = 10f;
    [Range(-500, 500)]
    public float topSpin = 0f;
    [Range(-500, 500)]
    public float sideSpin = 0f;
    public float spawnInterval = 5f;
    public UnityEvent onBallSpawned = new UnityEvent();

    // Private fields
    private Queue<BallRigidbodyBhv> _ballPool = new Queue<BallRigidbodyBhv>();
    private BallRigidbodyBhv _currentBall;

    private void Start()
    {
        if (ballPrefab != null)
        {
            for (int i = 0; i < ballPoolSize; ++i)
            {
                BallRigidbodyBhv newBall = Instantiate(ballPrefab, this.Position, this.Rotation, this.transform).GetComponent<BallRigidbodyBhv>();

                newBall.name = "Ball";

                newBall.Active = false;

                _ballPool.Enqueue(newBall);
            }
        }
    }

    private void Update()
    {
        if (Time.time % spawnInterval < Time.deltaTime)
        {
            this.SpawnBall();

            onBallSpawned.Invoke();
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
            this.ReturnObjectToPool(_currentBall);
        }

        _currentBall = this.GetBallFromPool();

        ballTracker.trackedTransform = _currentBall.transform;

        _currentBall.Move(this.Position, this.Rotation);

        _currentBall.LinearVelocity = this.Forward * linearSpeed;
        _currentBall.AngularVelocity = this.Right * topSpin + this.Up * sideSpin;

        TennisManager.Instance.Ball = _currentBall;
    }

    private BallRigidbodyBhv GetBallFromPool()
    {
        while (_ballPool.Count > 0)
        {
            BallRigidbodyBhv ball = _ballPool.Dequeue();

            if (ball != null)
            {
                ball.Active = true;

                return ball;
            }
            else
            {
                Debug.LogWarning("Found a null object in the pool. Has some code outside the pool destroyed it?");
            }
        }

        Debug.LogError("All pooled objects are already in use or have been destroyed");

        return null;
    }

    public void ReturnObjectToPool(BallRigidbodyBhv ball)
    {
        if (ball != null)
        {
            _ballPool.Enqueue(ball);
        }
    }
}
