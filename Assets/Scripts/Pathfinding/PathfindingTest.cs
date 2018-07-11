using System.Collections.Generic;
using UnityEngine;

public class PathfindingTest : MonoBehaviour
{
    public TileMap map;

    public bool DrawPaths = true;
    public float LineDuration = 0.5f;

    public bool Single = false;
    public int PathsPerFrame = 1;

    public void Update()
    {
        if (!map.IsLoaded())
            return;

        if (Single ? Input.GetKeyDown(KeyCode.B) : Input.GetKey(KeyCode.B))
        {
            for (int i = 0; i < PathsPerFrame; i++)
            {
                int x = Random.Range(0, map.Data.WidthInTiles);
                int y = Random.Range(0, map.Data.HeightInTiles);
                int ex = Random.Range(0, map.Data.WidthInTiles);
                int ey = Random.Range(0, map.Data.HeightInTiles);

                PathRequest.Create(x, y, ex, ey, Done);
            }
        }
    }

    private void Done(PathfindingResult result, List<PNode> path)
    {
        //Debug.Log("Path returned with result {0}, path has {1} nodes.".Form(result, path == null ? 0 : path.Count));

        if(path != null && DrawPaths)
        {
            DrawPath(path, LineDuration);
        }
    }

    private void DrawPath(List<PNode> path, float duration)
    {
        if (path == null)
            return;

        if (path.Count < 2)
            return;

        Vector2 start = new Vector2();
        Vector2 end = new Vector2();

        for (int i = 0; i < path.Count - 1; i++)
        {
            start.x = path[i].X;
            start.y = path[i].Y;
            end.x = path[i + 1].X;
            end.y = path[i + 1].Y;

            Debug.DrawLine(start, end, Color.cyan, duration);
        }
    }
}