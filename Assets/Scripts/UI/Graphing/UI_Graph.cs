using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UI_Graph : MonoBehaviour
{
    // A very simple graph rendering utility. The prefab is made to scale to small sizes.
    // Uses GL.LINES, which draws in the world layer and is therefore affected by post processing and is draw under UI. Pretty lame.
    // For debugging purposes.

    [Header("General")]
    public string Title = "My Graph";

    [Header("Axis")]
    public string XLabel = "Thing (samples)";
    public string YLabel = "Thing (unit)";

    [Header("Dividors")]
    public Color VerticalDividorColour = new Color(0f, 1f, 0f, 0.4f);
    public Color HorizontalDividorColour = new Color(0f, 1f, 0f, 0.4f);

    [Header("Background")]
    public Color BackgroundColour = Color.white;

    [Header("Controls")]
    [Range(2, 2000)]
    public int MaxSamples = 50;
    public bool AutoScale = true;
    public bool AutoSupportNegatives = true;
    public bool SupportNegatives = false;
    [Range(0.01f, 1000f)]
    public float Scale = 10f;
    public int AutoScaleAddition = 0;
    public int MinAutoScale = 0;

    [Header("References")]
    public Text TitleText;
    public Text XAxisText;
    public Text YAxisText;
    public RectTransform GraphArea;
    public Image VerticalDividor;
    public Image HorizontalDividor;
    public Text[] YAxisScales;
    public Text[] XAxisScales;

    [NonSerialized]
    private List<float> samples = new List<float>();

    public void Update()
    {
        UpdateAutoSystem();
        UpdateVisuals();
        DrawGraph();

        while (samples.Count >= MaxSamples)
        {
            samples.RemoveAt(0);
            if (samples.Count == 0)
                break;
        }
    }

    private void UpdateAutoSystem()
    {
        if (AutoScale)
        {
            if (!SupportNegatives)
            {
                if (samples.Count > 0)
                {
                    Scale = Mathf.Max(MinAutoScale, samples.Max() + AutoScaleAddition);
                }
            }
            else
            {
                if (samples.Count > 0)
                {
                    float max = samples.Max();
                    float min = samples.Min();
                    Scale = Mathf.Max(MinAutoScale, Mathf.Max(Mathf.Abs(max), Mathf.Abs(min)) + AutoScaleAddition);
                }
            }
        }

        if (AutoSupportNegatives)
        {
            if (samples.Count > 0)
            {
                SupportNegatives = samples.Min() < 0f;
            }
        }
    }

    private void UpdateVisuals()
    {
        HorizontalDividor.color = HorizontalDividorColour;
        VerticalDividor.color = VerticalDividorColour;

        XAxisText.text = XLabel;
        YAxisText.text = YLabel;

        TitleText.text = Title;

        // Y axis scale...
        if (!SupportNegatives)
        {
            YAxisScales[0].text = 0f.ToString();
            YAxisScales[1].text = (Scale / 2f).ToString("n1");
            YAxisScales[2].text = Scale.ToString("n1");
        }
        else
        {
            YAxisScales[0].text = (-Scale).ToString("n1");
            YAxisScales[1].text = 0f.ToString();
            YAxisScales[2].text = Scale.ToString("n1");
        }

        // X axis scale...
        XAxisScales[0].text = 0f.ToString();
        XAxisScales[1].text = Mathf.Round(samples.Count / 2f) == samples.Count / 2f ? Mathf.RoundToInt(samples.Count / 2f).ToString() : (samples.Count / 2f).ToString("n1");
        XAxisScales[2].text = samples.Count.ToString();
    }

    private void DrawGraph()
    {
        // Background
        Vector2 startingPos = GraphArea.position;
        Rect size = GraphArea.rect;
        CameraPrimitives.DrawQuad(new Rect(startingPos.x, startingPos.y, size.width, size.height), BackgroundColour);

        if (samples.Count < 2)
        {
            // Don't draw unless there are at least two points.
            return;
        }

        // Draw the current samples.
        Vector2 a = new Vector2();
        Vector2 b = new Vector2();        
        int total = samples.Count - 1;
        float yStart = SupportNegatives ? size.height / 2f : 0f;

        for (int i = 0; i < total; i++)
        {
            // Draw line from this point to the next point.
            // We need to work out the X and Y positions.

            // How many vertical pixels do we have?

            float sx = size.width * ((float)i / total);
            float ex = size.width * ((float)(i + 1) / total);
            float sy = yStart + size.height * (samples[i] / Scale * (SupportNegatives ? 0.5f : 1f));
            float ey = yStart + size.height * (samples[i + 1] / Scale * (SupportNegatives ? 0.5f : 1f));

            // Apply values.
            a.x = startingPos.x + sx;
            b.x = startingPos.x + ex;
            a.y = startingPos.y + sy;
            b.y = startingPos.y + ey;

            // Draw
            CameraPrimitives.DrawLine(a, b, Color.red);
        }        
    }

    public void AddSample(float value)
    {
        if (samples.Count == MaxSamples)
            samples.RemoveAt(0);
        samples.Add(value);        
    }

    public void ClearSamples()
    {
        samples.Clear();
    }
}