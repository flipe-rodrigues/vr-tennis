using UnityEngine;

public class TargetHitAudioFeedbackBhv : AudioFeedbackBhv
{
    private void OnEnable()
    {
        TargetBhv.onTargetHit += this.HandleTargetHit;
    }

    private void OnDisable()
    {
        TargetBhv.onTargetHit -= this.HandleTargetHit;
    }

    private void HandleTargetHit(TargetBhv target)
    {
        this.PlayClipAtPoint(target.Position, 1f);
    }
}
