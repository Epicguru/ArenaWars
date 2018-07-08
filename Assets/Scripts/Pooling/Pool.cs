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
            if(drain[id] >= TimeBeforeDrain)
            {
                bin.Add(id);
                Debug.Log("Binning {0}!".Form(id));
            }
        }

        foreach (var index in bin)
        {
            if (pool.ContainsKey(index))
            {
                foreach (var go in pool[index])
                {
                    Destroy(go.gameObject);
                }
                pool.Remove(index);
                if (groups.ContainsKey(index))
                {
                    Destroy(groups[index]);
                    groups.Remove(index);
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
                if(Instance.drain[id] >= Instance.TimeBeforeDrain)
                {
                    // Don't bother pooling.
                    Destroy(instance.gameObject);
                    return;
                }
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