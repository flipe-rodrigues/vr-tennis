using UnityEngine;
using UnityEditor;
using System;

public class ApplicationManager : Singleton<ApplicationManager>
{
    // Public properties
    public bool HasStartedQuitting => _hasStartedQuitting;

    // Static fields
    public static readonly WaitForFixedUpdate waitForFixedUpdateInstance = new WaitForFixedUpdate();
    public static Action onQuitStart;

    // Public fields
    public int targetFrameRate = 90;
    public int targetPhysicsRate = 1000;
    [Range(.01f, 1f)]
    public float timeScale = 1f;

    // Read only fields
    [SerializeField, ReadOnly]
    private bool _hasStartedQuitting = false;

    protected override void OnValidate()
    {
        base.OnValidate();

        Application.targetFrameRate = targetFrameRate;

        Time.fixedDeltaTime = 1f / targetPhysicsRate;

        Time.maximumDeltaTime = 1f / targetFrameRate;

        Time.timeScale = timeScale;
    }

    private void Start()
    {
        this.OnValidate();
    }

    private void LateUpdate()
    {
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            this.StartToQuit();
        }

        if (_hasStartedQuitting && TrackingManager.Instance.IsDoneSaving)
        {
            this.Quit();
        }
    }

    public void StartToQuit()
    {
        onQuitStart?.Invoke();

        _hasStartedQuitting = true;
    }

    private void Quit()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }
}
