using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugView : MonoBehaviour
{
    public static DebugView Instance;
    public static bool IsEnabled
    {
        get
        {
            return false && Instance != null && (Instance.ENABLED_IN_PLAYER ? true : Application.isEditor);
        }
    }

    public bool ENABLED_IN_PLAYER = false;

    [Header("Graphing")]
    public Dictionary<string, UI_Graph> Graphs = new Dictionary<string, UI_Graph>();
    [Range(0.1f, 2f)]
    [Tooltip("The Max Sample multiplier. Only applied when creating a graph.")]
    public float GraphSampleMultiplier = 1f;

    [SerializeField] private UI_Graph graphPrefab;
    [SerializeField] private Transform graphParent;

    public void Awake()
    {
        Instance = this;
    }

    public static UI_Graph CreateGraph(string key, string title, string xLabel, string yLabel, int maxSamples)
    {        
        if (Instance == null)
        {
            Debug.LogError("Graph cannot be created, debug view class not ready.");
            return null;
        }

        if (!IsEnabled)
        {
            Debug.LogWarning("Graphing is not enabled in player mode, returning null.");
            return null;
        }

        if (Instance.Graphs.ContainsKey(key))
        {
            Debug.LogError("Graph cannot be created, a graph with the same key ({0}) already exists!".Form(key));
            return null;
        }

        var spawned = Instantiate(Instance.graphPrefab);
        spawned.transform.SetParent(Instance.graphParent);
        spawned.Title = title;
        spawned.XLabel = xLabel;
        spawned.YLabel = yLabel;
        spawned.MaxSamples = Mathf.Max(2, Mathf.CeilToInt(maxSamples * Instance.GraphSampleMultiplier));

        Instance.Graphs.Add(key, spawned);

        return spawned;
    }

    public static void AddGraphSample(string key, float sample)
    {
        if (Instance == null)
        {
            Debug.LogError("Graph cannot be edited, debug view class not ready.");
            return;
        }

        if (!IsEnabled)
        {
            return;
        }

        var g = GetGraph(key);
        if(g != null)
        {
            g.AddSample(sample);
        }
    }

    public static void RemoveGraph(string key)
    {
        if (Instance == null)
        {
            Debug.LogError("Graph cannot be removed, debug view class not ready.");
            return;
        }

        if (!IsEnabled)
        {
            return;
        }

        var g = GetGraph(key);
        if(g != null)
        {
            Destroy(g.gameObject);
            Instance.Graphs.Remove(key);
        }
    }

    public static UI_Graph GetGraph(string key)
    {
        if (Instance == null)
        {
            Debug.LogError("Graph cannot be accessed, debug view class not ready.");
            return null;
        }

        if (!IsEnabled)
        {
            return null;
        }

        if (Instance.Graphs.ContainsKey(key))
        {
            return Instance.Graphs[key];
        }
        else
        {
            Debug.LogError("No graph found for key '{0}'. You have to call DebugView.CreateGraph() first and check spelling.");
            return null;
        }
    }
}
