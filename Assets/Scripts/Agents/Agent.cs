using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AgentMovement))]
[RequireComponent(typeof(NetPosSync))]
public class Agent : NetworkBehaviour
{
    public static List<Agent> All = new List<Agent>();

    [Header("Info")]
    [SyncVar]
    public string Name = "Agent-Default";

    [Header("References")]
    public AgentMovement Movement;
    public AgentRotation Rotation;
    public AgentHands Hands;

    public void Awake()
    {
        All.Add(this);
    }

    public void OnDestroy()
    {
        All.Remove(this);
    }

    public void Update()
    {
        gameObject.name = Name;
    }

    [Server]
    public void EquipItem(Item spawned, bool early = false)
    {
        if(spawned == null)
        {
            return;
        }

        // Un-equip old item.
        if(Hands.GetHeldItem() != null)
        {
            // Will destroy on server and all clients.
            Destroy(Hands.GetHeldItem().gameObject);
        }

        // Parent the spawned item to the hands.
        spawned.NetParenting.SetParent(Hands.HandsParent, early);
    }
}