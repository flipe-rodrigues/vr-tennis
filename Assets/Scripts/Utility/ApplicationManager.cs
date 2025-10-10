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
    [SerializeField, ReadOnly]
    private float _targetFrameInterval;
    [Tooltip("It seems this needs to be high for the ball's sake, not so much the collision")]
    public int targetPhysicsRate = 1000;
    public int minimumPhysicsRate = 250;
    [SerializeField, ReadOnly]
    private float _targetPhysicsInterval;
    [SerializeField, ReadOnly]
    private float _minimumPhysicsInterval;
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
        Time.maximumDeltaTime = 1f / minimumPhysicsRate;

        _targetFrameInterval = 1f / Application.targetFrameRate;
        _targetPhysicsInterval = Time.fixedDeltaTime;
        _minimumPhysicsInterval = Time.maximumDeltaTime;

        Time.timeScale = timeScale;
    }

    private void Start()
    {
        this.OnValidate();
    }

    private void LateUpdate()
    {
        if (!_hasStartedQuitting && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            this.StartToQuit();
        }

        if (_hasStartedQuitting && (TrackingManager.Instance.IsDoneSaving || !DataManager.Instance.saveData))
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
