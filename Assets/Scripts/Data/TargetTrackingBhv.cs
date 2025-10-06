using UnityEngine;

public class TargetTrackingBhv : TrackingBhv
{
    protected override void OnEnable()
    {
        base.OnEnable();

        TargetSelectionBhv.onTargetSelection += this.HandleTargetSelection;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        TargetSelectionBhv.onTargetSelection -= this.HandleTargetSelection;
    }

    private void HandleTargetSelection(TargetBhv target)
    {
        this.ReparentToCurrentTarget(target);
    }

    private void ReparentToCurrentTarget(TargetBhv target)
    {
        this.Transform.SetParent(target.Transform);

        this.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
