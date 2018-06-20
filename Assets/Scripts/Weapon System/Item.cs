using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetParenting))]
public class Item : NetworkBehaviour
{
    // An equipable item. Agents can hold these items.

    public NetParenting NetParenting
    {
        get
        {
            if (_netParenting == null)
            {
                _netParenting = GetComponent<NetParenting>();
            }
            return _netParenting;
        }
    }
    private NetParenting _netParenting;

    public Weapon Weapon
    {
        get
        {
            if(_weapon == null)
            {
                _weapon = GetComponent<Weapon>();
            }
            return _weapon;
        }
    }
    private Weapon _weapon;

    public bool IsWeapon
    {
        get
        {
            return Weapon != null;
        }
    }

    public Agent GetParentAgent()
    {
        if (NetParenting.CurrentParent != null)
        {
            var agent = NetParenting.CurrentParent.GetComponentInParent<Agent>();
            return agent;
        }
        else
        {
            return null;
        }
    }

    public static Dictionary<string, Item> loaded;

    public static void LoadAll()
    {
        if (loaded != null)
            return;

        loaded = new Dictionary<string, Item>();

        var items = Resources.LoadAll<Item>("Items");

        foreach(var i in items)
        {
            if (loaded.ContainsKey(i.name))
            {
                Debug.LogError("Duplicate item name: '{0}'! Only one of the items with that name has been loaded.".Form(i.name));
            }
            else
            {
                loaded.Add(i.name, i);
            }
        }

        Debug.Log("Loaded {0} items.".Form(loaded.Count));
    }

    public static void UnloadAll()
    {
        if (loaded == null)
            return;

        loaded.Clear();
        loaded = null;
    }

    public static void NetRegisterAll()
    {
        if (loaded == null)
            return;

        foreach(var pair in loaded)
        {
            NetworkPrefabs.Add(pair.Value.gameObject);
        }
    }

    public static bool IsLoaded(string name)
    {
        return loaded != null && loaded.ContainsKey(name);
    }

    public static Item Get(string name)
    {
        if (!IsLoaded(name))
        {
            Debug.LogError("An item for the name '{0}' is not loaded! Check spelling!".Form(name));
            return null;
        }
        else
        {
            return loaded[name];
        }
    }

    [Server]
    public static Item NetSpawn(Item prefab)
    {
        if (!NetworkServer.active)
            return null;

        if (prefab == null)
            return null;

        var spawned = Instantiate(prefab);
        NetworkServer.Spawn(spawned.gameObject);

        return spawned;
    }

    [Server]
    public static Item NetSpawnAndEquip(Item prefab, Agent agent)
    {
        if (!NetworkServer.active)
            return null;

        if (prefab == null)
            return null;

        if (agent == null)
            return null;

        var spawned = Instantiate(prefab);

        agent.EquipItem(spawned, true);

        NetworkServer.Spawn(spawned.gameObject);

        return spawned;
    }
}