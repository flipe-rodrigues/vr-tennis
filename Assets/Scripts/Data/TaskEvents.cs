using System.Collections.Generic;

public enum TaskEventType
{
    None,
    StageStart,
    TrialStart,
    BallLaunch,
    CourtEnter,
    CourtExit,
    NetEnter,
    NetExit,
    TargetEnter,
    TargetHit,
    TargetExit,
    RacketHit,
    BallOutOfPlay,
}

public static class TaskEvents
{
    public static readonly Dictionary<TaskEventType, string> Map = new()
    {
        { TaskEventType.None, "" },
        { TaskEventType.StageStart, "StageStart" },
        { TaskEventType.TrialStart, "TrialStart" },
        { TaskEventType.BallLaunch, "BallLaunch" },
        { TaskEventType.CourtEnter, "CourtEnter" },
        { TaskEventType.CourtExit, "CourtExit" },
        { TaskEventType.NetEnter, "NetEnter" },
        { TaskEventType.NetExit, "NetExit" },
        { TaskEventType.TargetEnter, "TargetEnter" },
        { TaskEventType.TargetHit, "TargetHit" },
        { TaskEventType.TargetExit, "TargetExit" },
        { TaskEventType.RacketHit, "RacketHit" },
        { TaskEventType.BallOutOfPlay, "BallOutOfPlay" },
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
