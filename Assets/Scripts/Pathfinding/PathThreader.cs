
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class PathThreader : MonoBehaviour
{
    // Does the multithreaded part of pathfinding, using the Pathfinding and PathRequest classes.
    [System.NonSerialized] public Thread[] Threads;
    [System.NonSerialized] public System.Diagnostics.Stopwatch[] Watches;
    [System.NonSerialized] public Pathfinding[] Pathers;
    [System.NonSerialized] public Queue<UnityAction<PathfindingResult, List<PNode>>> ToReturn;
    [System.NonSerialized] public Queue<PathfindingResult> ToReturn_Results;
    [System.NonSerialized] public Queue<List<PNode>> ToReturn_Paths;
    public long[] ExecutionTimes;

    [Header("References")]
    public TileMap Map;

    [Header("Controls")]
    public int ThreadCount = 1;
    public int SleepTime = 5;

    public static object LOK = new object();
    public static object LOK_2 = new object();
    private bool run = false;
    private const string EXEC_TIME = "PathExecTime";
    private const string PENDING = "PathPending";

    public void Awake()
    {
        // Debugging graphs
        var g = DebugView.CreateGraph(EXEC_TIME, "Avg Pathfinding Time", "Seconds Ago", "% of Second Executing", 2 * 60);
        if(g != null)
        {
            g.AutoScale = false;
            g.Scale = 1f;

            InvokeRepeating("UpdateGraphs", 1f, 1f);
        }
        DebugView.CreateGraph(PENDING, "Pending Path Requests", "Seconds Ago", "Request Count", 2 * 60);

        ToReturn = new Queue<UnityAction<PathfindingResult, List<PNode>>>();
        ToReturn_Paths = new Queue<List<PNode>>();
        ToReturn_Results = new Queue<PathfindingResult>();

        // Create the new threads.
        Threads = new Thread[ThreadCount];
        Watches = new System.Diagnostics.Stopwatch[ThreadCount];
        Pathers = new Pathfinding[ThreadCount];
        ExecutionTimes = new long[ThreadCount];

        run = true;
        for (int i = 0; i < ThreadCount; i++)
        {
            Watches[i] = new System.Diagnostics.Stopwatch();
            Pathers[i] = new Pathfinding();
            Threads[i] = new Thread(Run);
            Threads[i].Start(i);
        }

        Debug.Log("Started {0} pathfinding threads.".Form(ThreadCount));
    }

    public void Update()
    {
        lock (ToReturn)
        {
            while (ToReturn.Count > 0)
            {
                var action = ToReturn.Dequeue();
                var path = ToReturn_Paths.Dequeue();
                var result = ToReturn_Results.Dequeue();

                action.Invoke(result, path);
            }
        }
    }

    private void UpdateGraphs()
    {
        if (DebugView.IsEnabled == false)
            return;

        float sum = 0f;
        for (int i = 0; i < ExecutionTimes.Length; i++)
        {
            sum += ExecutionTimes[i] / 1000f;
            ExecutionTimes[i] = 0;
        }
        float avg = sum / ExecutionTimes.Length;

        DebugView.AddGraphSample(EXEC_TIME, avg);
        DebugView.AddGraphSample(PENDING, PathRequest.Pending.Count);
    }

    public void OnDestroy()
    {
        // Stop the thread.
        run = false;
        Threads = null;
    }

    private void Run(object index)
    {
        int threadNumber = (int)index;
        int cyclesIdle = 0;
        const int CYCLES_BEFORE_CLEAR = 5000;

        while (run)
        {
            try
            {
                int count = PathRequest.Pending.Count;
                if (count == 0)
                {
                    // If there is not work to do, just wait to avoid mashing the CPU.
                    Thread.Sleep(SleepTime);
                    cyclesIdle++;
                    if(cyclesIdle == CYCLES_BEFORE_CLEAR)
                    {
                        Pathers[threadNumber].Clear();
                    }
                }
                else
                {
                    cyclesIdle = 0;
                    Watches[threadNumber].Restart();

                    // Get the first object, locking at the same time, and the process it.
                    PathRequest req;
                    lock (LOK)
                    {
                        // Possible for the first item to be cancelled between the above if statement and here...
                        if (PathRequest.Pending.Count == 0)
                            continue;

                        req = PathRequest.Pending[0];
                        PathRequest.Pending.RemoveAt(0);
                    }

                    if(req.UponProcessed == null)
                    {
                        Watches[threadNumber].Stop();
                        continue;
                    }

                    req.FlagWorking();

                    List<PNode> path = null;
                    PathfindingResult result;
                    try
                    {
                        result = Pathers[threadNumber].Run(req.StartX, req.StartY, req.EndX, req.EndY, Map, out path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception in pathfinding ->execution<- on thread #{0}:".Form(threadNumber));
                        Debug.LogError(e);
                        result = PathfindingResult.ERROR_INTERNAL;
                        path = null;
                    }

                    req.FlagIdle();
                    lock (LOK_2)
                    {
                        PathRequest.IdlePool.Enqueue(req);
                    }

                    // Give result.
                    if (req.UponProcessed != null)
                    {
                        lock (ToReturn)
                        {
                            ToReturn_Paths.Enqueue(path);
                            ToReturn_Results.Enqueue(result);
                            ToReturn.Enqueue(req.UponProcessed);
                        }
                    }

                    Watches[threadNumber].Stop();
                    long time = Watches[threadNumber].ElapsedMilliseconds;
                    ExecutionTimes[threadNumber] += time;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in pathfinding thread #{0}:".Form(threadNumber));
                Debug.LogError(e);
            }            
        }
    }
}