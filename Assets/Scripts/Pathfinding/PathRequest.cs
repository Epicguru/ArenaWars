
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PathRequest
{
    public static List<PathRequest> Pending = new List<PathRequest>();
    public static Queue<PathRequest> IdlePool = new Queue<PathRequest>();

    public int StartX { get; private set; }
    public int StartY { get; private set; }
    public int EndX { get; private set; }
    public int EndY { get; private set; }

    public PathRequestState State { get; private set; }

    public UnityAction<PathfindingResult, List<PNode>> UponProcessed { get; private set; }

    public static PathRequest Create(int startX, int startY, int endX, int endY, UnityAction<PathfindingResult, List<PNode>> uponProcessed)
    {
        if(uponProcessed == null)
        {
            Debug.LogWarning("The UponProcessed action is null, request ignored! Returning null.");
            return null;
        }

        if(IdlePool.Count > 0)
        {
            lock (PathThreader.LOK_2)
            {
                var got = IdlePool.Dequeue();

                got.StartX = startX;
                got.StartY = startY;
                got.EndX = endX;
                got.EndY = endY;
                got.UponProcessed = uponProcessed;
                got.State = PathRequestState.REQUESTED;

                if (!Pending.Contains(got))
                {
                    Pending.Add(got);
                }
                else
                {
                    Debug.LogError("A request object in the idle queue also existed in the pending queue! How and why?");
                    return null;
                }

                return got;
            }
        }
        else
        {
            var got = new PathRequest();

            got.StartX = startX;
            got.StartY = startY;
            got.EndX = endX;
            got.EndY = endY;
            got.UponProcessed = uponProcessed;
            got.State = PathRequestState.REQUESTED;

            Pending.Add(got);

            return got;
        }
    }

    private PathRequest()
    {

    }

    /// <summary>
    /// Cancels the pathfinding request. Once this has been called. Do not attept to call other methods on this same object.
    /// To create a new request after this has been called, use PathRequest.Create() to make a new request.
    /// </summary>
    public void Cancel()
    {
        // Add it back into the idle pool.
        if (IdlePool.Contains(this))
        {
            Debug.LogError("This pathfinding request has already been cancelled, or was never started.");
            return;
        }

        lock (PathThreader.LOK)
        {
            // Remove from the pending list.
            Pending.Remove(this);

            // Also set the UponProcessed to null to avoid it being called if this is current processing. A waste, but oh well.
            UponProcessed = null;
        }

        IdlePool.Enqueue(this);
        State = PathRequestState.IDLE;        
    }

    /// <summary>
    /// Internal method. Do not call.
    /// </summary>
    public void FlagWorking()
    {
        State = PathRequestState.PROCESSING;
    }

    /// <summary>
    /// Internal method. Do not call.
    /// </summary>
    public void FlagIdle()
    {
        State = PathRequestState.IDLE;
    }
}