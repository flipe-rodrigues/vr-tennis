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

    private void HandleBallLaunch()
    {
        this.ReparentToCurrentBall();
    }

    private void ReparentToCurrentBall()
    {
        this.Transform.SetParent(TennisManager.Instance.Ball.Transform);

        this.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
