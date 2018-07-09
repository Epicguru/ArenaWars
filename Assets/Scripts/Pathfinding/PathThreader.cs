
using System.Threading;
using UnityEngine;

public class PathThreader : MonoBehaviour
{
    // Does the multithreaded part of pathfinding, using the Pathfinding and PathRequest classes.
    [System.NonSerialized]
    public Thread thread;

    public int SleepTime = 5;

    private bool run = false;

    public void Awake()
    {
        // Create the new thread.
        thread = new Thread(Run);
        thread.Start();
    }

    public void OnDestroy()
    {
        // Stop the thread.
        run = false;
        thread = null;
    }

    private void Run()
    {
        Debug.Log("Started pathfinding thread.");

        while (run)
        {

            // If there is not work to do, just wait to avoid mashing the CPU.
            Thread.Sleep(SleepTime);
        }

        Debug.Log("Shut down pathfinding thread.");
    }
}