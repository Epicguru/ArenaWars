
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PathRequest
{
    public static List<PathRequest> Pending = new List<PathRequest>();
    private static Queue<PathRequest> idlePool = new Queue<PathRequest>();

    public int StartX { get; private set; }
    public int StartY { get; private set; }

    public PathRequestState State { get; private set; }

    public UnityAction<PathfindingResult, List<PNode>> UponProcessed { get; private set; }

    public static PathRequest Create(int startX, int startY, UnityAction<PathfindingResult, List<PNode>> uponProcessed)
    {
        if(uponProcessed == null)
        {
            Debug.LogWarning("The UponProcessed action is null, request ignored! Returning null.");
            return null;
        }

        if(idlePool.Count > 0)
        {
            var got = idlePool.Dequeue();

            got.StartX = startX;
            got.StartY = startY;
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
        else
        {
            var got = new PathRequest();

            got.StartX = startX;
            got.StartY = startY;
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
        if (idlePool.Contains(this))
        {
            Debug.LogError("This pathfinding request has already been cancelled, or was never started.");
            return;
        }

        idlePool.Enqueue(this);
        State = PathRequestState.IDLE;
    }
}