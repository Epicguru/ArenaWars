
using UnityEngine;

public class Memory : MonoBehaviour
{
    private static Memory instance;

    public float AssetUnloadDelay = 0.25f;
    public bool GraphMemory = true;

    private bool unloadUA;
    private float timeToUA;
    private const string MEMORY_USE = "MemoryUsage";

    public static void UnloadUnusedAssets()
    {
        instance.unloadUA = true;
        instance.timeToUA = 0f;
    }

    public static void GC()
    {
        // Just do normal collection.
        System.GC.Collect();
    }

    public void Awake()
    {
        instance = this;

        if (GraphMemory)
        {
            DebugView.CreateGraph(MEMORY_USE, "Total Memory", "Seconds Ago", "Megabytes", 2 * 60);
            InvokeRepeating("AddGraphSample", 1f, 1f);
        }
    }

    public void Update()
    {
        if (unloadUA)
        {
            timeToUA += Time.unscaledDeltaTime;
            if(timeToUA >= AssetUnloadDelay)
            {
                unloadUA = false;
                timeToUA = 0f;

                Resources.UnloadUnusedAssets();
            }
        }
    }

    private void AddGraphSample()
    {
        if (!GraphMemory)
            return;

        DebugView.AddGraphSample(MEMORY_USE, System.GC.GetTotalMemory(false) / 1024f / 1024f);
    }
}