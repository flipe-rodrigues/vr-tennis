using System.Collections.Generic;

public enum TaskEventType
{
    None,
    TrialStart,
    BallSpawn,
    CourtEnter,
    CourtExit,
    NetEnter,
    NetExit,
    TargetEnter,
    TargetExit,
    RacketHit,
}

public static class TaskEvents
{
    public static readonly Dictionary<TaskEventType, string> Map = new()
    {
        { TaskEventType.None, "" },
        { TaskEventType.TrialStart, "TrialStart" },
        { TaskEventType.BallSpawn, "BallSpawn" },
        { TaskEventType.CourtEnter, "CourtEnter" },
        { TaskEventType.CourtExit, "CourtExit" },
        { TaskEventType.NetEnter, "NetEnter" },
        { TaskEventType.NetExit, "NetExit" },
        { TaskEventType.TargetEnter, "TargetEnter" },
        { TaskEventType.TargetExit, "TargetExit" },
        { TaskEventType.RacketHit, "RacketHit" },
    };

    public static int GetCode(this TaskEventType eventType)
    {
        return (int)eventType;
    }

    public static string GetName(this TaskEventType eventType)
    {
        return TaskEvents.Map[eventType];
    }
}
