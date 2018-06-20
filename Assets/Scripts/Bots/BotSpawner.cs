using UnityEngine;
using UnityEngine.Networking;

public class BotSpawner : NetworkBehaviour
{
    public Bot Prefab;
    public PoolableObject Region;

    public override void OnStartServer()
    {
        var spawned = Instantiate(Prefab);
        spawned.transform.position = (Vector2)transform.position + Random.insideUnitCircle * 3f;

        NetworkServer.Spawn(spawned.gameObject);

        Pool.Get(Region);
    }
}
