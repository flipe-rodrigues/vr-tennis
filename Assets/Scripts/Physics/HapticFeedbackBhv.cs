using Oculus.Haptics;

public class HapticFeedbackBhv : FeedbackBhv
{
    protected override void Play()
    {
        float relativeVelocity = (TennisManager.Instance.Ball.LinearVelocity - TennisManager.Instance.Racket.LinearVelocity).magnitude;

         float amplitude = relativeVelocity * amplitudeModifier;

        HapticsManager.Instance.Play(amplitude);
    }
}
