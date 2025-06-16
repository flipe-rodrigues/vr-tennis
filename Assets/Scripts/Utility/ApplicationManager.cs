using UnityEngine;
using UnityEditor;
using System;

public class ApplicationManager : Singleton<ApplicationManager>
{
    // Public properties
    public bool HasStartedQuitting => _hasStartedQuitting;

    // Static fields
    public static readonly WaitForFixedUpdate waitForFixedUpdateInstance = new WaitForFixedUpdate();
    public static Action onQuitRequest;

    // Public fields
    [Range(.01f, 1f)]
    public float timeScale = 1f;
    public int targetFrameRate = 120;

    // Read only fields
    [SerializeField, ReadOnly]
    private bool _hasStartedQuitting = false;

    protected override void OnValidate()
    {
        base.OnValidate();

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
            onQuitRequest?.Invoke();

            _hasStartedQuitting = true;
        }

        if (_hasStartedQuitting && TrackingManager.Instance.IsDoneSaving)
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
}
