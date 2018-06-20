using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkPrefabs
{
    private static List<GameObject> Buffer = new List<GameObject>();

    public static void Add(GameObject prefab)
    {
        if (!Buffer.Contains(prefab))
        {
            Buffer.Add(prefab);
        }
    }

    public static void Apply()
    {
        if (Buffer.Count == 0)
            return;

        foreach (var go in Buffer)
        {
            ClientScene.RegisterPrefab(go);
        }
        Buffer.Clear();
    }
}