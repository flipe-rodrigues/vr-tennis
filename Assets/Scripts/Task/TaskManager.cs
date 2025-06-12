using UnityEngine;

public class TaskManager : Singleton<TaskManager>
{
    // Public properties
    public int StageIndex => _stageIndex;
    public int TrialIndex => _trialIndex;

    // Private properties
    private int StageTrialCount => trialsPerStage * (_stageIndex + 1);

    // References
    public MeshRenderer courtMeshRenderer;
    public GameObject net;
    public GameObject targetZone;

    // Public fields
    [Header("Task Settings")]
    [Min(1)]
    public int trialsPerStage = 10;
    [Min(.01f)]
    public float interTrialInterval = 3;

    // Read only fields
    [SerializeField, ReadOnly]
    private int _stageIndex;
    [SerializeField, ReadOnly]
    private int _trialIndex;

    private void Start()
    {
        if (courtMeshRenderer != null)
        {
            courtMeshRenderer.enabled = false;
        }

        if (net != null)
        {
            net.SetActive(false);
        }

        if (targetZone != null)
        {
            targetZone.SetActive(false);
        }

    }

    public void StartTrial()
    {
        TrackingManager.Instance.RecordEvent("TrialStart");

        if (_trialIndex > this.StageTrialCount && _stageIndex == 0)
        {
            if (net != null)
            {
                net.SetActive(true);
            }

            if (courtMeshRenderer != null)
            {
                courtMeshRenderer.enabled = true;
            }

            _stageIndex++;
        }

        if (_trialIndex > this.StageTrialCount && _stageIndex == 1)
        {
            if (targetZone != null) 
            { 
                targetZone.SetActive(true);
            }

            _stageIndex++;
        }

        _trialIndex++;
    }
}