using UnityEngine;
using UnityEditor;
using System;

public enum QuestFrameRates
{
    _72Hz = 72,
    _90Hz = 90,
    _120Hz = 120,
}

public class ApplicationManager : Singleton<ApplicationManager>
{
    // Public properties
    public bool HasStartedQuitting => _hasStartedQuitting;

    // Static fields
    public static readonly WaitForFixedUpdate waitForFixedUpdateInstance = new WaitForFixedUpdate();
    public static Action onQuitStart;

    // Public fields
    public QuestFrameRates questFrameRate = QuestFrameRates._72Hz;
    [SerializeField, ReadOnly]
    private float _deltaTime;
    [Range(50, 1000)]
    public int targetPhysicsRate = 1000;
    [SerializeField, ReadOnly]
    private float _fixedDeltaTime;
    [SerializeField, ReadOnly]
    private float _physicsStepsPerFrame;
    [SerializeField, ReadOnly]
    private float _maximumAllowedTimestep;
    [Range(.01f, 1f)]
    public float timeScale = 1f;

    // Read only fields
    [SerializeField, ReadOnly]
    private bool _hasStartedQuitting = false;

    protected override void OnValidate()
    {
        base.OnValidate();

        Application.targetFrameRate = (int)questFrameRate;

        _physicsStepsPerFrame = MathF.Ceiling((float)targetPhysicsRate / (float)Application.targetFrameRate);
        Time.fixedDeltaTime = 1f / targetPhysicsRate;
        Time.maximumDeltaTime = 1f / _physicsStepsPerFrame;

        _deltaTime = 1f / Application.targetFrameRate;
        _fixedDeltaTime = Time.fixedDeltaTime;
        _maximumAllowedTimestep = Time.maximumDeltaTime;

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
