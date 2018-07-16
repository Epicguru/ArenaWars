using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Bot))]
public class BotNavigation : NetworkBehaviour
{
    private static Vector2 HALF = Vector2.one / 2f;

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

    public Vector2Int CurrentPathTarget
    {
        get; private set;
    }

    [System.NonSerialized]
    private List<PNode> path = new List<PNode>();

    [SerializeField]
    [ReadOnly]
    private int pathRebuildCount;

    public void Update()
    {
        bool rebuild = PathNeedsRebuild();
        if (rebuild)
        {
            // Are we already rebuilding? Then don't do anything until that is done.
            // Cancelling could lead to a path never being found.

            bool req = true;

            if (Request != null && Request.State != PathRequestState.IDLE)
                req = false;

            if (req)
            {
                RequestNewPath(AproxTilePos.x, AproxTilePos.y, TargetPos.x, TargetPos.y);
            }
        }
    }

    private void RequestNewPath(int x, int y, int tx, int ty)
    {
        if (Request != null)
        {
            if(Request.State != PathRequestState.IDLE)
            {
                Debug.LogError("New path requested when the last path was in queue or processing! Not valid!");
                return;
            }
        }

        Request = PathRequest.Create(x, y, tx, ty, PathDone);
    }

    public bool PathNeedsRebuild()
    {
        bool pathExists = path != null;
        bool targetMoved = TargetPos != CurrentPathTarget;

        return !pathExists || targetMoved;
    }

    private void PathDone(PathfindingResult result, List<PNode> fullPath)
    {
        if(result != PathfindingResult.SUCCESSFUL)
        {
            // Did not succeed? Oh well.
            return;
        }
        if (fullPath == null)
        {
            // Path was not built, should never be true when sucessful is true but we check anyway.
            return;
        }

        CurrentPathTarget = (Vector2Int)fullPath[fullPath.Count - 1];
        path = fullPath;
        pathRebuildCount++;
    }

    public void OnDrawGizmosSelected()
    {
        if (path == null || path.Count < 2)
            return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i] + HALF, path[i + 1] + HALF);
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