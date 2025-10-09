using UnityEngine;

public class TargetAudioFeedbackBhv : AudioFeedbackBhv
{
    private void OnEnable()
    {
        TargetBhv.onTargetAcquiredV2 += this.PlayClipAtPoint;
    }

    private void OnDisable()
    {
        TargetBhv.onTargetAcquiredV2 -= this.PlayClipAtPoint;
    }
}
