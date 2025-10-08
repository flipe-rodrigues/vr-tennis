using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class TaskStage
{
    [Min(0)]
    public float duration = Mathf.Infinity;
    [Min(1)]
    public int trialCount = 10;
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;
}

public class TaskManager : Singleton<TaskManager>
{
    // Static fields
    public static event Action onTrialStart;

    // Public properties
    public int StageIndex => _stageIndex;
    public int TrialIndex => _trialIndex;

    // Public fields
    public TruncatedExponentialDistribution itiDistribution = new TruncatedExponentialDistribution(3, 4, 9);
    public List<TaskStage> stages;

    // Readonly fields
    [SerializeField, ReadOnly]
    public float _interTrialInterval = 3f;
    [SerializeField, ReadOnly]
    private int _stageIndex = 0;
    [SerializeField, ReadOnly]
    private int _trialIndex = 0;
    [SerializeField, ReadOnly]
    private int _totalTrialCount;
    [SerializeField, ReadOnly]
    private Timer _stageTimer;

    // Private fields
    private float _lastTrialStartTime = -Mathf.Infinity;
    private int[] _stageTransitionThresholds;

    protected override void OnValidate()
    {
        base.OnValidate();

        itiDistribution.UpdatePDF();
    }

    private void Start()
    {
        _stageTransitionThresholds = new int[stages.Count + 1];

        _stageTransitionThresholds[0] = 0;

        _totalTrialCount = stages[0].trialCount;

        for (int i = 1; i < stages.Count; i++)
        {
            _stageTransitionThresholds[i] = _stageTransitionThresholds[i - 1] + stages[i - 1].trialCount;

            _totalTrialCount += stages[i].trialCount;
        }

        _stageTransitionThresholds[stages.Count] = int.MaxValue;

        this.StartNextTrial();
    }

    private void FixedUpdate()
    {
        if (_trialIndex >= _stageTransitionThresholds[_stageIndex] || _stageTimer.IsExpired)
        {
            this.StartStage();
        }

        if (Time.time - _lastTrialStartTime >= _interTrialInterval)
        {
            //if (_stageIndex == 3)
                this.StartNextTrial();
        }

        if (_trialIndex >= _totalTrialCount)
        {
            ApplicationManager.Instance.StartToQuit();

            this.enabled = false;
        }
    }
    private void StartStage()
    {
        _stageTimer = new Timer(stages[_stageIndex].duration);
        _stageTimer.Start();

        foreach (GameObject obj in stages[_stageIndex].objectsToEnable)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in stages[_stageIndex].objectsToDisable)
        {
            obj.SetActive(false);
        }

        _stageIndex++;
    }

    public void StartNextTrial()
    {
        _lastTrialStartTime = Time.time;

        _trialIndex++;

        _interTrialInterval = itiDistribution.Sample();

        onTrialStart?.Invoke();

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TrialStart);
    }
}