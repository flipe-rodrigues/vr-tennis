using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public properties
    public bool IsSaving
    {
        get
        {
            for (int i = 0; i < _trackers.Count; i++)
            {
                if (_trackers[i].IsSaving)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // Read only fields
    [SerializeField, ReadOnly]
    private List<TrackingBhv> _trackers;
    [SerializeField, ReadOnly]
    private int _expectedDataCount;

    protected override void OnValidate()
    {
        base.OnValidate();

        _trackers = FindObjectsByType<TrackingBhv>(FindObjectsSortMode.None).ToList();

        _expectedDataCount = Mathf.CeilToInt(TaskManager.Instance.interTrialInterval / Time.fixedDeltaTime * 1.05f);
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();

        foreach (var tracker in _trackers)
        {
            tracker.InitializeDataList(_expectedDataCount);
        }
    }

    private void OnEnable()
    {
        TaskManager.onTrialEnd += this.HandleTrialEnd;
    }

    private void OnDisable()
    {
        TaskManager.onTrialEnd -= this.HandleTrialEnd;
    }

    private void HandleTrialEnd()
    {
        this.SaveAndClear();
    }

    public void SaveAndClear()
    {
        foreach (var tracker in _trackers)
        {
            if (DataManager.Instance.saveData)
            {
                tracker.SaveAndClear();
            }
            else
            {
                tracker.Clear();
            }
        }
    }

    public void RecordEvent(TaskEventType taskEvent) 
    {
        if (!DataManager.Instance.saveData)
        {
            return;
        }

        foreach (var tracker in _trackers)
        {
            tracker.Record(taskEvent);
        }
    }
}

