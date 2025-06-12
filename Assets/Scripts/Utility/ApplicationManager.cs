using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    // Static fields

    public static readonly WaitForFixedUpdate waitForFixedUpdateInstance = new WaitForFixedUpdate();

    // Public fields
    [Range(.01f, 1f)]
    public float timeScale = 1f;
    public int targetFrameRate = 120;

    private void OnValidate()
    {
        Time.timeScale = timeScale;

        Application.targetFrameRate = targetFrameRate;
    }

    private void Start()
    {
        this.OnValidate();
    }

    private void LateUpdate()
    {
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
    }
}
