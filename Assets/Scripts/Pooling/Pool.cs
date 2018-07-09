using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public static Pool Instance;

    private Dictionary<int, Queue<PoolableObject>> pool = new Dictionary<int, Queue<PoolableObject>>();
    private Dictionary<int, GameObject> groups = new Dictionary<int, GameObject>();
    private Dictionary<int, float> drain = new Dictionary<int, float>();
    private List<int> bin = new List<int>();

    public bool Drain = false;
    public float TimeBeforeDrain = 10f;
    public float DrainInterval = 1f;

    [Header("Debug")]
    [ReadOnly]
    public int CurrentlyPooled;
    [ReadOnly]
    public int BorrowedPerFrame;
    [ReadOnly]
    public int SpawnedPerFrame;

    private const string POOLED = "PoolTotal";
    private const string BORROWED = "PoolBorrowed";
    private const string SPAWNED = "PoolSpawned";

    private float timer;

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        InvokeRepeating("DrainOne", 0f, DrainInterval);

        DebugView.CreateGraph(POOLED, "Pooled Objects", "Seconds Ago", "Object Count", 120);
        DebugView.CreateGraph(SPAWNED, "Pool Spawned Objects", "Frames Ago", "Spawn Count", 1000).MinAutoScale = 20;
        DebugView.CreateGraph(BORROWED, "Pool Borrowed Objects", "Frames Ago", "Borrow Count", 1000).MinAutoScale = 20;
    }

    public void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    public void Update()
    {
        if (!Drain)
            return;

        float dt = Time.unscaledDeltaTime;
        foreach (var id in drain.Keys.ToArray())
        {
            drain[id] += dt;            
        }        
    }

    private void LateUpdate()
    {
        bool fire = false;
        timer += Time.unscaledDeltaTime;
        if (timer >= 1f)
        {
            timer -= 1f;
            fire = true;
        }

        if (fire)
        {
            CurrentlyPooled = 0;
            foreach (var pair in pool)
            {
                CurrentlyPooled += pair.Value.Count;
            }
        }

        if (DebugView.IsEnabled)
        {
            DebugView.AddGraphSample(SPAWNED, SpawnedPerFrame);
            DebugView.AddGraphSample(BORROWED, BorrowedPerFrame);            

            if (fire)
            {
                DebugView.AddGraphSample(POOLED, CurrentlyPooled);
            }            
        }

        BorrowedPerFrame = 0;
        SpawnedPerFrame = 0;
    }

    private void DrainOne()
    {
        if (!Drain)
            return;

        foreach (var id in drain.Keys.ToArray())
        {
            if (drain[id] >= TimeBeforeDrain)
            {
                bin.Add(id);
            }
        }
        
        foreach (var index in bin)
        {
            if (pool.ContainsKey(index))
            {
                int count = pool[index].Count;
                if (count > 1)
                {
                    var obj = pool[index].Dequeue();
                    Destroy(obj.gameObject);
                }
                else
                {
                    var obj = pool[index].Dequeue();
                    Destroy(obj.gameObject);
                    pool.Remove(index);

                    if (groups.ContainsKey(index))
                    {
                        Destroy(groups[index]);
                        groups.Remove(index);
                    }
                }
            }
            else
            {
                drain.Remove(index);
            }
        }

        bin.Clear();
    }

    private static void Ensure(int id)
    {
        if (Instance == null)
            return;

        Dictionary<int, Queue<PoolableObject>> p = Instance.pool;
        if (!p.ContainsKey(id))
        {
            p.Add(id, new Queue<PoolableObject>());
        }
        else
        {
            if(p[id] == null)
            {
                p[id] = new Queue<PoolableObject>();
            }
        }
    }

    private static PoolableObject GetFromPool(int id)
    {
        if (Instance == null)
            return null;

        var p = Instance.pool;
        PoolableObject found = null;
        var pid = p[id];
        while (found == null)
        {
            if (pid.Count == 0)
                break;
            found = pid.Dequeue();
        }

        return found;        
    }

    private static bool ContainsPooled(int id)
    {
        if (Instance == null)
            return false;

        var p = Instance.pool;

        if (!p.ContainsKey(id))
        {
            return false;
        }
        else
        {
            var pid = p[id];
            if (pid == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    private static PoolableObject CreateNew(PoolableObject prefab)
    {
        if (prefab == null)
            return null;

        int id = prefab.gameObject.GetInstanceID();

        PoolableObject spawned = Instantiate(prefab);
        spawned.Setup(id);

        return spawned;
    }

    private static GameObject GetGroup(int id)
    {
        if (Instance == null)
            return null;

        var g = Instance.groups;
        if (g.ContainsKey(id))
        {
            if(g[id] == null)
            {
                g[id] = new GameObject("#" + id);
                g[id].transform.parent = Instance.gameObject.transform;
            }   
            
            return g[id];
        }
        else
        {
            g.Add(id, new GameObject("#" + id));
            g[id].transform.parent = Instance.gameObject.transform;
            return g[id];
        }
    }

    public static GameObject Get(PoolableObject prefab)
    {
        return Get(prefab, Vector3.zero, Quaternion.identity, null);
    }

    public static GameObject Get(PoolableObject prefab, Vector3 position)
    {
        return Get(prefab, position, Quaternion.identity, null);
    }

    public static GameObject Get(PoolableObject prefab, Vector3 position, Quaternion rotation)
    {
        return Get(prefab, position, rotation, null);
    }

    public static GameObject Get(PoolableObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if(prefab == null)
        {
            Debug.LogError("Null prefab, cannot spawn or borrow from pool!");
            return null;
        }
        if(Instance == null)
        {
            Debug.LogError("Cannot borrow or create object, pool instance is null!");
            return null;
        }

        int id = prefab.gameObject.GetInstanceID();

        if (ContainsPooled(id))
        {
            var fromPool = GetFromPool(id);
            if(fromPool != null)
            {
                fromPool.Spawn(position, rotation, parent);

                // Debug stats
                Instance.BorrowedPerFrame++;

                return fromPool.gameObject;
            }
        }

        // Reset the drain timer, if active.
        if (Instance.drain.ContainsKey(id))
        {
            Instance.drain[id] = 0f;
        }
        else
        {
            Instance.drain.Add(id, 0f);
        }        

        var created = CreateNew(prefab);
        if(created != null)
        {
            // Debug stats
            Instance.SpawnedPerFrame++;

            created.Spawn(position, rotation, parent);
            return created.gameObject;
        }
        else
        {
            Debug.LogError("All creation and pool extraction methods failed, something is very wrong...");
            return null;
        }
    }

    public static void Return(PoolableObject instance)
    {
        if (instance == null)
            return;
        if (Instance == null)
        {
            Debug.LogError("Cannot return to pool, pool instance is null!");
            return;
        }

        int id = instance.PrefabID;
        instance.Despawn();

        if (Instance.Drain)
        {
            if (Instance.drain.ContainsKey(id))
            {
                Instance.drain[id] = 0f;
            }
            else
            {
                Instance.drain.Add(id, 0f);
            }
        }

        instance.gameObject.SetActive(false);
        instance.transform.SetParent(GetGroup(id).transform, !instance.IsUI);

        Ensure(id);
        Instance.pool[id].Enqueue(instance);
    }
}