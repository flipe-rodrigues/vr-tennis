using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    // Public fields
    [Range(.01f, 1f)]
    public float timeScale = 1f;
    public int targetFrameRate = 120;

    private void Start()
    {
        Time.timeScale = timeScale;

        Application.targetFrameRate = targetFrameRate;
    }

    private void LateUpdate()
    {
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
    }
}
