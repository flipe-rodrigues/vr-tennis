using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public properties
    public int ExpectedDataSize => _expectedDataCount;
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
    }

    public void SaveAndClear()
    {
        foreach (var tracker in _trackers)
        {
            if (SaveSystem.Instance.saveData)
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
        if (!SaveSystem.Instance.saveData)
        {
            return;
        }

        foreach (var tracker in _trackers)
        {
            tracker.RecordEvent(eventName);
        }
    }
}

