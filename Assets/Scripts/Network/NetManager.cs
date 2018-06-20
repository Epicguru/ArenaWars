
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetManager : NetworkManager
{
    public Agent PlayerAgentPrefab;

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Client has connected to server: IP: {0}, connection ID: {1}. Welcome!".Form(conn.address, conn.connectionId));
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        var go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        // Setup player. For now just give them a faction and a name based on their player number.
        Player player = go.GetComponent<Player>();
        player.Name = "Player #" + Player.All.Count;
        player.IP_Adress = conn.address;

        // Create a new agent for the player.
        Agent agent = Instantiate(PlayerAgentPrefab);
        agent.Name = player.Name + " - Agent";
        agent.transform.position = Random.insideUnitCircle * 2f;

        NetworkServer.Spawn(agent.gameObject);

        player.SetAgent(agent);

        // Give the agent an assault rifle.
        Item.NetSpawnAndEquip(Item.Get(Random.value >= 0.5f ? "Assault Rifle" : "Dual SMG"), agent);

        NetworkServer.AddPlayerForConnection(conn, go, playerControllerId);
    }
}