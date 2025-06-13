using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public properties
    public bool IsSaving => _trackers.Any(tracker => tracker.IsSaving);

    // Read only fields
    [SerializeField, ReadOnly]
    private List<TrackingBhv> _trackers;
    [SerializeField, ReadOnly]
    private int _expectedDataCount;

    public void OnValidate()
    {
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

    public void RecordEvent(string eventName) 
    {
        if (!DataManager.Instance.saveData)
        {
            return;
        }

        foreach (var tracker in _trackers)
        {
            tracker.RecordEvent(eventName);
        }
    }
}

