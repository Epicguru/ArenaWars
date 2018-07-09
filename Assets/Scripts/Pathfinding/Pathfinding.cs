﻿using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    public const int MAX = 1000;
    private const float DIAGONAL_DST = 1.41421356237f;

    private static FastPriorityQueue<PNode> open = new FastPriorityQueue<PNode>(MAX);
    private static Dictionary<PNode, PNode> cameFrom = new Dictionary<PNode, PNode>();
    private static Dictionary<PNode, float> costSoFar = new Dictionary<PNode, float>();
    private static List<PNode> near = new List<PNode>();
    private static bool left, right, below, above;

    public static PathfindingResult Run(int startX, int startY, int endX, int endY, TileMap map, out List<PNode> path)
    {
        if(map == null)
        {
            Debug.LogError("Null map, cannot pathfind.");
            path = null;
            return PathfindingResult.ERROR_INTERNAL;
        }

        // Validate start and end points.
        if(!map.TileInBounds(startX, startY))
        {
            path = null;
            return PathfindingResult.ERROR_START_OUT_OF_BOUNDS;
        }
        if (!map.TileInBounds(endX, endY))
        {
            path = null;
            return PathfindingResult.ERROR_END_OUT_OF_BOUNDS;
        }

        // Clear everything up.
        Clear();

        var start = PNode.Create(startX, startY);
        var end = PNode.Create(endX, endY);

        // Check the start/end relationship.
        if (start.Equals(end))
        {
            path = null;
            return PathfindingResult.ERROR_START_IS_END;
        }

        // Add the starting point to all relevant structures.
        open.Enqueue(start, 0f);
        cameFrom[start] = start;
        costSoFar[start] = 0f;

        int count;
        while((count = open.Count) > 0)
        {
            if(count >= MAX)
            {
                path = null;
                return PathfindingResult.ERROR_PATH_TOO_LONG;
            }

            var current = open.Dequeue();

            if (current.Equals(end))
            {
                // We found the end of the path!
                path = TracePath(end);
                return PathfindingResult.SUCCESSFUL;
            }

            var neighbours = GetNear(current, map);
            foreach (PNode n in neighbours)
            {
                float newCost = costSoFar[current] + GetCost(current, n); // Note that this could change depending on speed changes per-tile.

                if(!costSoFar.ContainsKey(n) || newCost < costSoFar[n])
                {
                    costSoFar[n] = newCost;
                    float priority = newCost + Heuristic(current, n);
                }
            }
        }

        path = null;
        return PathfindingResult.ERROR_INTERNAL;
    }

    private static List<PNode> TracePath(PNode end)
    {
        List<PNode> path = new List<PNode>();
        PNode child = end;

        bool run = true;
        while (run)
        {
            PNode previous = cameFrom[child];
            path.Add(child);
            if (previous != null && child != previous)
            {
                child = previous;
            }
            else
            {
                run = false;
            }
        }

        path.Reverse();
        return path;
    }

    public static void Clear()
    {
        costSoFar.Clear();
        cameFrom.Clear();
        near.Clear();
    }

    private static float Heuristic(PNode a, PNode b)
    {
        // Gives a rough distance.
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    private static float GetCost(PNode a, PNode b)
    {
        // Only intended for neighbours.

        // Is directly horzontal
        if (Mathf.Abs(a.X - b.X) == 1 && a.Y == b.Y)
        {
            return 1;
        }

        // Directly vertical.
        if (Mathf.Abs(a.Y - b.Y) == 1 && a.X == b.X)
        {
            return 1;
        }

        // Assume that it is on one of the corners.
        return DIAGONAL_DST;
    }

    private static List<PNode> GetNear(PNode node, TileMap map)
    {
        // Want to add nodes connected to the center node, if they are walkable.
        // This code stops the pathfinder from cutting corners, and going through walls that are diagonal from each other.

        near.Clear();

        // Left
        left = false;
        if (map.IsSpotWalkable(node.X - 1, node.Y))
        {
            near.Add(PNode.Create(node.X - 1, node.Y));
            left = true;
        }

        // Right
        right = false;
        if (map.IsSpotWalkable(node.X + 1, node.Y))
        {
            near.Add(PNode.Create(node.X + 1, node.Y));
            right = true;
        }

        // Above
        above = false;
        if (map.IsSpotWalkable(node.X, node.Y + 1))
        {
            near.Add(PNode.Create(node.X, node.Y + 1));
            above = true;
        }

        // Below
        below = false;
        if (map.IsSpotWalkable(node.X, node.Y - 1))
        {
            near.Add(PNode.Create(node.X, node.Y - 1));
            below = true;
        }

        // Above-Left
        if (left && above)
        {
            if (map.IsSpotWalkable(node.X - 1, node.Y + 1))
            {
                near.Add(PNode.Create(node.X - 1, node.Y + 1));
            }
        }

        // Above-Right
        if (right && above)
        {
            if (map.IsSpotWalkable(node.X + 1, node.Y + 1))
            {
                near.Add(PNode.Create(node.X + 1, node.Y + 1));
            }
        }

        // Below-Left
        if (left && below)
        {
            if (map.IsSpotWalkable(node.X - 1, node.Y - 1))
            {
                near.Add(PNode.Create(node.X - 1, node.Y - 1));
            }
        }

        // Below-Right
        if (right && below)
        {
            if (map.IsSpotWalkable(node.X + 1, node.Y - 1))
            {
                near.Add(PNode.Create(node.X + 1, node.Y - 1));
            }
        }

        return near;
    }
}