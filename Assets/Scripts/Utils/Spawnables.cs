using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Spawnables : NetworkBehaviour
{
    public static Spawnables Instance;

    public Projectile Projectile;

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    [ClientRpc]
    public void RpcSpawnTempEffect(byte effect, Vector2 position, float angle)
    {
        TempEffects e = (TempEffects)effect;

        TempEffect.NetCallback(e, position, angle);
    }
}