using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

[System.Serializable]
public class TaskStage
{
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
    [Min(.01f)]
    public float interTrialInterval = 3;
    public List<TaskStage> stages;

    // Read only fields
    [SerializeField, ReadOnly]
    private int _stageIndex = 0;
    [SerializeField, ReadOnly]
    private int _trialIndex = 0;
    [SerializeField, ReadOnly]
    private int _totalTrialCount;

    // Private fields
    private float _lastTrialStartTime = -Mathf.Infinity;
    [SerializeField, ReadOnly]
    private int[] _stageTransitionThresholds;

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
    }

    private void FixedUpdate()
    {
        if (_trialIndex >= _stageTransitionThresholds[_stageIndex])
        {
            this.StartStage();
        }

        if (Time.time - _lastTrialStartTime >= interTrialInterval)
        {
            this.StartTrial();
        }

        if (_trialIndex >= _totalTrialCount)
        {
            ApplicationManager.Instance.StartToQuit();

            this.enabled = false;
        }
    }
    private void StartStage()
    {
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

    private void StartTrial()
    {
        _lastTrialStartTime = Time.time;

        _trialIndex++;

        onTrialStart?.Invoke();

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TrialStart);
    }
}