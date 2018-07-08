
using UnityEngine;

public class Memory : MonoBehaviour
{
    private static Memory instance;

    public float AssetUnloadDelay = 0.25f;

    private bool unloadUA;
    private float timeToUA;

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
}