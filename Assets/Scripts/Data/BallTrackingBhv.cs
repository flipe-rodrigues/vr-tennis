using UnityEngine;

public class BallTrackingBhv : TrackingBhv
{
    public void ReparentToCurrentBall()
    {
        this.Transform.SetParent(TennisManager.Instance.Ball.Transform, worldPositionStays: false);
    }
}
