
using System.Collections.Generic;
using UnityEngine;

public class CameraLines : MonoBehaviour
{
    public static CameraLines Instance;

    public Material Mat;

    private List<Vector2> points = new List<Vector2>();
    private List<Color> colours = new List<Color>();

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void OnPostRender()
    {
        GL.PushMatrix();
        Mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.LINES);
        int x = 0;
        for (int i = 0; i < points.Count; i += 2)
        {
            int j = i + 1;
            GL.Color(colours[x++]);
            GL.Vertex(points[i]);
            GL.Vertex(points[j]);
        }
        GL.End();
        GL.PopMatrix();

        points.Clear();
        colours.Clear();
    }

    public static void DrawLine(Vector2 start, Vector2 end)
    {
        DrawLine(start, end, Color.white);
    }

    public static void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        if (Instance == null)
            return;

        Instance.points.Add(start);
        Instance.points.Add(end);
        Instance.colours.Add(colour);
    }
}