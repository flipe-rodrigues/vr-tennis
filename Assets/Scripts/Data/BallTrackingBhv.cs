using UnityEngine;

public class BallTrackingBhv : TrackingBhv
{
    private void OnEnable()
    {
        BallLaunchBhv.onBallLaunch += this.HandleBallLaunch;
    }

    private void OnDisable()
    {
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
