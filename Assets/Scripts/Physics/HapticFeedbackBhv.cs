using UnityEngine;
using UnityEngine.XR;

public class HapticFeedbackBhv : MonoBehaviour
{
    // Public fields
    public InputDeviceCharacteristics controllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    [Range(0, 1)]
    public float amplitudeModifier = 0.1f;

    [SerializeField, ReadOnly]
    private InputDevice targetDevice;

    public void Play(float relativeSpeed)
    {
        float amplitude = relativeSpeed * amplitudeModifier;

        HapticsManager.Instance.Play(amplitude);
    }
}
