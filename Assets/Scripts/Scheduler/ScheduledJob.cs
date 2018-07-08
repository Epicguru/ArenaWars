
using UnityEngine.Events;

public class ScheduledJob
{
    public JobState State = JobState.IDLE;
    public object[] Arguments = null;
    public UnityAction Action = null;
    public UnityAction UponCompletion;

    public bool IsValid
    {
        get
        {
            return Action != null;
        }
    }
}

public enum JobState : byte
{
    IDLE,
    PENDING,
    DONE,
    CANCELLED
}