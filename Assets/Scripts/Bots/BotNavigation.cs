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

    public bool HasControl = true;

    public Transform Target;

    public Vector2Int TargetPos;
    [Range(0.05f, 1f)]
    public float TargetNodeDeadzone = 0.2f;

    [Range(0.5f, 2f)]
    public float TargetNodeMaxDistance = 1.5f;

    [Range(0.1f, 2f)]
    public float TargetDeadzone = 0.5f;

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

    public bool ReachedTarget
    {
        get
        {
            if (path == null)
                return false;
            if (Vector2.Distance(transform.position, TargetPos + HALF) <= TargetDeadzone)
                return true;

            return false;
        }
    }

    [System.NonSerialized]
    private List<PNode> path = new List<PNode>();

    [SerializeField]
    [ReadOnly]
    private int pathRebuildCount;

    [SerializeField]
    [ReadOnly]
    [TextArea(5, 5)]
    private string debug;

    public void Update()
    {
        if (!isServer)
            return;

        if(Target != null)
        {
            TargetPos = new Vector2Int(Mathf.RoundToInt(Target.transform.position.x), Mathf.RoundToInt(Target.transform.position.y));
        }

        debug = "Has Control: {0}" + '\n' +
            "Path: {1}" + '\n' +
            "Reached Target: {2}" + '\n' +
            "Target Node: {3}" + '\n' +
            "Request State: {4}";
        debug = debug.Form(HasControl, path == null ? "null" : path.Count.ToString(), ReachedTarget, GetTargetNode() == null ? "null" : GetTargetNode().ToString(), Request == null ? "null" : Request.State.ToString());

        if (!HasControl)
        {
            path = null;
            if(Request != null && Request.State != PathRequestState.IDLE)
            {
                Request.Cancel();
            }
            return;
        }

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

        if(path != null && !ReachedTarget)
        {
            // Compare distance between the first and second node, to make slightly more intelligent descisions.
            if(path.Count >= 2)
            {
                float dst = Vector2.Distance(transform.position, path[0] + HALF);
                float dst2 = Vector2.Distance(transform.position, path[1] + HALF);

                if(dst2 < dst)
                {
                    // Second node is closer than first, remove first!
                    path.RemoveAt(0);
                }
            }

            // Move along path. Once the distance from this bot to the target node is smaller than
            // a certain value, it is removed from the list. Once there are no more nodes in the path,
            // we have reached the end.

            // If the distance to the next node is greater than Y, the path is recalculated.
            PNode targetNode = GetTargetNode();

            if (targetNode != null)
            {
                Vector2 pos = targetNode + HALF;
                Vector2 dir = pos - (Vector2)transform.position;
                SetBotDirection(dir);

                float dst = GetDistanceToTargetNode();
                if(dst <= TargetNodeDeadzone)
                {
                    // Remove the node it from the list...
                    path.RemoveAt(0);
                }
            }
            else
            {
                SetBotDirection(Vector2.zero);
            }
        }
        else
        {
            SetBotDirection(Vector2.zero);
        }
    }

    private void SetBotDirection(Vector2 dir)
    {
        Bot.Agent.Movement.InputDirection = dir;
    }

    public PNode GetTargetNode()
    {
        if (path == null || path.Count == 0)
            return null;

        return path[0];
    }

    public float GetDistanceToTargetNode()
    {
        var tn = GetTargetNode();
        if (tn == null)
            return 0f;
        return Vector2.Distance(tn + HALF, transform.position);
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
        if (!HasControl)
        {
            Debug.LogError("New bot path was requested when the bot naviagtion system was not in control!");
        }

        Request = PathRequest.Create(x, y, tx, ty, PathDone);
    }

    public bool PathNeedsRebuild()
    {
        if (!HasControl)
            return false;
        if (!isServer)
            return false;
        if (ReachedTarget)
            return false;

        if (path == null)
            return true;
        if (TargetPos != CurrentPathTarget)
            return true;
        var tn = GetTargetNode();
        if(tn != null)
        {
            if (GetDistanceToTargetNode() > TargetNodeMaxDistance)
                return true;
        }
        else
        {
            return true;
        }

        return false;
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

        var c = Gizmos.color;
        Gizmos.color = Color.white;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i] + HALF, path[i + 1] + HALF);
        }

        var targetNode = GetTargetNode();
        if (targetNode == null)
        {
            Gizmos.color = c;
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetNode + HALF, TargetNodeMaxDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetNode + HALF, TargetNodeDeadzone);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(TargetPos + HALF, TargetDeadzone);

        Gizmos.color = c;
    }

    public void OnDestroy()
    {
        if(Request != null && Request.State != PathRequestState.IDLE)
        {
            Request.Cancel();
        }
    }
}