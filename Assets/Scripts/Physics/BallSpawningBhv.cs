using UnityEngine;

public class BallSpawningBhv : CachedTransformBhv
{
    // Public fields
    public GameObject ballPrefab;
    [Range(0, 100)]
    public float linearSpeed = 10f;
    [Range(-500, 500)]
    public float topSpin = 0f;
    [Range(-500, 500)]
    public float sideSpin = 0f;
    public float spawnInterval = 5f;

    private void Update()
    {
        if (Time.time % spawnInterval < Time.deltaTime)
        {
            this.SpawnBall();
        }
    }

    private void SpawnBall()
    {
        if (ballPrefab == null)
        {
            return;
        }

        BallRigidbodyBhv ball = Instantiate(ballPrefab, this.Position, this.Rotation).GetComponent<BallRigidbodyBhv>();

        ball.LinearVelocity = this.Forward * linearSpeed;
        ball.AngularVelocity = this.Right * topSpin + this.Up * sideSpin;

        TennisManager.Instance.Ball = ball;
    }
}
