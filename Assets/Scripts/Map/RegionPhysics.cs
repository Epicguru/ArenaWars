using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionPhysics : MonoBehaviour
{
    public Region Region;

    public Transform ColliderParent;
    public PoolableObject Prefab;

    [System.NonSerialized]
    public List<PoolableObject> InUse = new List<PoolableObject>();

    public void Build()
    {
        // Spawn new colliders...
        int sX = Region.X * Region.SIZE;
        int sY = Region.Y * Region.SIZE;
        for (int x = 0; x < Region.SIZE; x++)
        {
            for (int y = 0; y < Region.SIZE; y++)
            {
                int index = TileMap.Instance.GetTileIndex(sX + x, sY + y);
                ushort id = TileMap.Instance.TileIDs[index];
                if(id != 0)
                {
                    // Assume that everything that is not air is solid!
                    InUse.Add(Pool.Get(Prefab, Region.transform.position + new Vector3(x, y, 0f), Quaternion.identity, ColliderParent).GetComponent<PoolableObject>());
                }
            }
        }        
    }

    public void Despawn()
    {
        if (InUse.Count > 0)
        {
            foreach (var c in InUse)
            {
                if (c != null)
                {
                    Pool.Return(c);
                }
            }
            InUse.Clear();
        }
    }
}