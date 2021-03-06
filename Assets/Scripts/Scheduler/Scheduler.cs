﻿using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Scheduler : MonoBehaviour
{
    public static Scheduler Instance;

    [Header("Controls")]
    public long MAX_TIME_PER_FRAME = 3L;

    [Header("Stats")]
    [ReadOnly]
    [Tooltip("The number of pending jobs remaining after last frame's executing.")]
    public int Pending;
    [ReadOnly]
    [Tooltip("The number of jobs executed last frame. Excludes any cancelled or invalid jobs.")]
    public uint ProcessedLastFrame;
    [ReadOnly]
    [Tooltip("The amount of milliseconds that were used to processes scheduled jobs last frame. Should always be less or equal to the MAX_TIME_PER_FRAME value.")]
    public long ElapsedMillisecondsLastFrame;
    [ReadOnly]
    [Tooltip("The total number of jobs executed, over the whole runtime. Excludes any cancelled or invalid jobs.")]
    public uint Processed;
    [ReadOnly]
    [Tooltip("The total number of jobs cancelled after they were scheduled, over the whole runtime. Does not include invalid jobs.")]
    public uint Cancelled;

    [System.NonSerialized]
    public Queue<ScheduledJob> Jobs = new Queue<ScheduledJob>();
    private Stopwatch sw = new Stopwatch();

    private const string PROCESSED = "JobsProcessed";
    private const string TIME = "JobsTime";
    private const string PENDING = "JobsPending";

    public void Awake()
    {
        Instance = this;

        DebugView.CreateGraph(PROCESSED, "Jobs Processed", "Frame", "Job Count in Frame", 1000);
        var time = DebugView.CreateGraph(TIME, "Jobs Execution Time", "Frame", "Total Job Time in Frame", 1000);
        if(time != null)
        {
            time.MinAutoScale = 10;
        }
        DebugView.CreateGraph(PENDING, "Pending Jobs", "Frame", "Total Pending Jobs", 1000);
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public static void AddJob(ScheduledJob job)
    {
        if (job == null)
        {
            UnityEngine.Debug.LogWarning("Job is null, nothing scheduled.");
            return;
        }
        if (job.State != JobState.IDLE)
        {
            UnityEngine.Debug.LogWarning("Invalid state to schedule job: {0}".Form(job.State));
            return;
        }

        job.State = JobState.PENDING;
        Instance.Jobs.Enqueue(job);
    }

    public void Update()
    {
        sw.Restart();
        ProcessedLastFrame = 0;

        while (true)
        {
            if (Jobs.Count == 0)
                break;

            var job = Jobs.Dequeue();
            if(job.State == JobState.DONE || job.State == JobState.CANCELLED || !job.IsValid)
            {
                if (job.State == JobState.CANCELLED)
                    Cancelled++;
                continue;
            }

            if(job.Arguments == null || job.Arguments.Length == 0)
            {
                job.Action.Invoke();
            }
            else
            {
                job.Action.DynamicInvoke(job.Arguments);
            }

            if(job.UponCompletion != null)
            {
                job.UponCompletion.Invoke();
            }

            job.State = JobState.DONE;
            Processed++;
            ProcessedLastFrame++;

            if(sw.ElapsedMilliseconds >= MAX_TIME_PER_FRAME)
            {
                break;
            }
        }

        sw.Stop();
        ElapsedMillisecondsLastFrame = sw.ElapsedMilliseconds;
        Pending = Jobs.Count;

        UpdateDebugView();
    }

    private void UpdateDebugView()
    {
        if (!DebugView.IsEnabled)
            return;

        DebugView.AddGraphSample(PROCESSED, ProcessedLastFrame);
        DebugView.AddGraphSample(TIME, ElapsedMillisecondsLastFrame);
        DebugView.AddGraphSample(PENDING, Pending);
    }
}