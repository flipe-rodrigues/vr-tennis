using UnityEngine;

public class BallTrackingBhv : TrackingBhv
{
    protected override void OnEnable()
    {
        base.OnEnable();

        BallLaunchBhv.onBallLaunch += this.HandleBallLaunch;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        BallLaunchBhv.onBallLaunch -= this.HandleBallLaunch;
    }

    private void HandleBallLaunch(BallBhv ball)
    {
        this.ReparentToCurrentBall(ball);
    }

    private void ReparentToCurrentBall(BallBhv ball)
    {
        this.Transform.SetParent(ball.Transform);
        this.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
