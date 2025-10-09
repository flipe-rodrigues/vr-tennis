using UnityEngine;
using System.Collections.Generic;
using System;

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
    public List<TaskStage> stages;
    public TruncatedExponentialDistribution ITIDistribution = new TruncatedExponentialDistribution(3, 4, 9);

    // Readonly fields
    [SerializeField, ReadOnly]
    private Timer _ITITimer;
    [SerializeField, ReadOnly]
    private int _stageIndex = 0;
    [SerializeField, ReadOnly]
    private int _trialIndex = 0;
    [SerializeField, ReadOnly]
    private int _totalTrialCount;

    // Private fields
    [SerializeField, ReadOnly]
    private int[] _stageTransitionThresholds;

    private void OnEnable()
    {
        TargetBhv.onTargetHit += HandleTargetHit;
        BallBhv.onBallOutOfPlay += HandleBallSecondBounce;
    }

    private void HandleTargetHit(TargetBhv target)
    {
        this.EndTrial();
    }

    private void HandleBallSecondBounce(BallBhv ball)
    {
        this.EndTrial();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        ITIDistribution.UpdatePDF();
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
        _ITITimer = new Timer(ITIDistribution.mean);
    }

    private void FixedUpdate()
    {
        if (_trialIndex >= _stageTransitionThresholds[_stageIndex])
        {
            this.StartStage();
        }

        if (_ITITimer.IsExpired)
        {
            this.StartTrial();
            _ITITimer.Stop();
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
        this.StartTrial();

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.StageStart);
    }

    private void StartTrial()
    {
        _trialIndex++;
        _ITITimer.duration = ITIDistribution.Sample();
        onTrialStart?.Invoke();

        TrackingManager.Instance.RecordTaskEvent(TaskEventType.TrialStart);
    }

    private void EndTrial()
    {
        _ITITimer.Start();
    }
}