using UnityEngine;
using System;

public class TaskManager : Singleton<TaskManager>
{
    // Static fields
    public static event Action onTrialStart;

    // Public properties
    public int StageIndex => _stageIndex;
    public int TrialIndex => _trialIndex;

    // Private properties
    private int StageTrialCount => trialsPerStage * (_stageIndex + 1);

    // Public fields
    [Min(1)]
    public int trialsPerStage = 10;
    [Min(.01f)]
    public float interTrialInterval = 3;

    // Read only fields
    [SerializeField, ReadOnly]
    private int _stageIndex;
    [SerializeField, ReadOnly]
    private int _trialIndex;

    // Private fields
    private CourtBhv _court;
    private NetBhv _net;
    private TargetBhv _target;
    private float _lastTrialStartTime = -Mathf.Infinity;

    protected override void Awake()
    {
        base.Awake();

        _court = this.GetComponentInChildren<CourtBhv>();
        _net = this.GetComponentInChildren<NetBhv>();
        _target = this.GetComponentInChildren<TargetBhv>();
    }

    private void Start()
    {
        _court.MeshRenderer.enabled = false;
        _net.Active = false;
        _target.Active = false;
    }

    private void FixedUpdate()
    {
        if (Time.time - _lastTrialStartTime >= interTrialInterval)
        {
            this.StartTrial();
        }
    }

    private void StartTrial()
    {
        if (_trialIndex > this.StageTrialCount && _stageIndex == 0)
        {
            _net.Active = true;

            _court.MeshRenderer.enabled = true;

            _stageIndex++;
        }

        if (_trialIndex > this.StageTrialCount && _stageIndex == 1)
        {
            _target.Active = true;

            _stageIndex++;
        }

        _lastTrialStartTime = Time.time;

        _trialIndex++;

        onTrialStart?.Invoke();

        TrackingManager.Instance.RecordEvent(TaskEventType.TrialStart);
    }
}