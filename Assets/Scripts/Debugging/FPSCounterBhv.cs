using UnityEngine;
using TMPro;

public class FPSCounterBhv : MonoBehaviour
{
    // Private fields
    private TextMeshProUGUI _fpsText;

    private void Awake()
    {
        _fpsText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (_fpsText != null)
        {
            float fps = 1.0f / Time.deltaTime;

            _fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
        }
    }
}
