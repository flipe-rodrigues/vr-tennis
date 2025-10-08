using UnityEngine;

public class TargetTrackingBhv : TrackingBhv
{
    public void ReparentToCurrentTarget(TargetBhv target)
    {
        this.Transform.SetParent(target.Transform);

        this.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
