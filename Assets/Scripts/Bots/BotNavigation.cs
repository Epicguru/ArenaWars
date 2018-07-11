using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Bot))]
public class BotNavigation : NetworkBehaviour
{
    private static Vector2 HALF = Vector2.one / 2f;
    private const int PATH_START_LENGTH = 5;

    public Bot Bot
    {
        get
        {
            if(_bot == null)
            {
                _bot = GetComponent<Bot>();
            }
            return _bot;
        }
    }
    private Bot _bot;

    public Vector2Int TargetPos;

    public PathRequest Request { get; private set; }
    public Vector2Int AproxTilePos
    {
        get
        {
            _aproxPos.x = Mathf.FloorToInt(transform.position.x);
            _aproxPos.y = Mathf.FloorToInt(transform.position.y);
            return _aproxPos;
        }
    }
    private Vector2Int _aproxPos = new Vector2Int();

    public PathRequestState state;

    [System.NonSerialized]
    private List<PNode> pathStart = new List<PNode>();

    public void Update()
    {        
        if(Request == null || Request.State == PathRequestState.IDLE)
        {
            if(pathStart == null || pathStart.Count <= 2)
            {
                var current = this.AproxTilePos;
                Request = PathRequest.Create(current.x, current.y, TargetPos.x, TargetPos.y, PathDone);
            }            
        }

        if(Request != null)
        {
            state = Request.State;
        }

        if(pathStart != null && pathStart.Count > 0)
        {
            Bot.Agent.Movement.InputDirection = (pathStart[0] + HALF) - (Vector2)transform.position;
            float dst = Vector2.Distance(pathStart[0] + HALF, (Vector2)transform.position);
            const int FRAME_COUNT = 2;
            if (dst <= (Bot.Agent.Movement.FinalSpeed * Time.deltaTime) * FRAME_COUNT)
            {
                // We will reach this point the next FRAME_COUNT frames.
                // Move on to the next point by removing the first point.
                pathStart.RemoveAt(0);
            }
        }
    }

    private void PathDone(PathfindingResult result, List<PNode> fullPath)
    {
        if(result != PathfindingResult.SUCCESSFUL)
        {
            return;
        }
        if(fullPath == null)
        {
            return;
        }

        int len = (PATH_START_LENGTH > fullPath.Count ? fullPath.Count : PATH_START_LENGTH);
        pathStart.Clear();

        // Copy the full path to the limited path, because we don't need all of it, just the next few points.
        for (int i = 0; i < len; i++)
        {
            pathStart.Add(fullPath[i]);
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (pathStart == null || pathStart.Count < 2)
            return;

        for (int i = 0; i < pathStart.Count - 1; i++)
        {
            Gizmos.DrawLine(pathStart[i] + HALF, pathStart[i + 1] + HALF);
        }
    }

    public void OnDestroy()
    {
        if(Request != null && Request.State != PathRequestState.IDLE)
        {
            Request.Cancel();
        }
    }
}