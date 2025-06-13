using UnityEngine;

public class BallTrackingBhv : TrackingBhv
{
    public void ReparentToCurrentBall()
    {
        this.Transform.SetParent(TennisManager.Instance.Ball.Transform);

        this.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
