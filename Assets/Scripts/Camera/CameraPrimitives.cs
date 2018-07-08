
using System.Collections.Generic;
using UnityEngine;

public class CameraPrimitives : MonoBehaviour
{
    public static CameraPrimitives Instance;

    public Material Mat;

    private List<Vector2> linePoints = new List<Vector2>();
    private List<Color> lineColours = new List<Color>();
    private List<Rect> quads = new List<Rect>();
    private List<Color> quadColours = new List<Color>();

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

        // Quads
        GL.Begin(GL.QUADS);
        for (int i = 0; i < quads.Count; i++)
        {
            GL.Color(quadColours[i]);
            var r = quads[i];

            GL.Vertex3(r.xMin, r.yMin, 0f);
            GL.Vertex3(r.xMin, r.yMax, 0f);
            GL.Vertex3(r.xMax, r.yMax, 0f);
            GL.Vertex3(r.xMax, r.yMin, 0f);
        }
        GL.End();

        // Lines
        GL.Begin(GL.LINES);
        int x = 0;
        for (int i = 0; i < linePoints.Count; i += 2)
        {
            int j = i + 1;
            GL.Color(lineColours[x++]);
            GL.Vertex(linePoints[i]);
            GL.Vertex(linePoints[j]);
        }
        GL.End();        
        GL.PopMatrix();

        linePoints.Clear();
        lineColours.Clear();
        quads.Clear();
        quadColours.Clear();
    }

    public static void DrawLine(Vector2 start, Vector2 end)
    {
        DrawLine(start, end, Color.white);
    }

    public static void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        if (Instance == null)
            return;

        Instance.linePoints.Add(start);
        Instance.linePoints.Add(end);
        Instance.lineColours.Add(colour);
    }

    public static void DrawQuad(Rect rect)
    {
        DrawQuad(rect, Color.white);
    }

    public static void DrawQuad(Rect rect, Color colour)
    {
        if (Instance == null)
            return;

        Instance.quads.Add(rect);
        Instance.quadColours.Add(colour);
    }
}