using UnityEngine;

public class RacketHitAudioFeedbackBhv : AudioFeedbackBhv
{
    private void OnEnable()
    {
        RacketColliderBhv.onRacketHit += this.HandleRacketHit;
    }

    private void OnDisable()
    {
        RacketColliderBhv.onRacketHit -= this.HandleRacketHit;
    }

    private void HandleRacketHit()
    {
        this.PlayClipAtPoint(TennisManager.Instance.Ball.Position, TennisManager.Instance.RelativeVelocity.magnitude);
    }
}
