using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    // Attached to the camera, floats around following whatever Agent it is controlling.
    public static List<Player> All = new List<Player>();

    public Agent Agent { get; private set; }

    [Header("Info")]
    [SyncVar]
    public string Name = "Player-Default";
    [SerializeField]
    [ReadOnly]
    [SyncVar]
    private uint AgentID;

    [Header("Debug")]
    [ReadOnly]
    public string IP_Adress;

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

        // Find agent from networked ID.
        FindAgent();

        // Local player only from now on.
        if (!isLocalPlayer)
            return;

        // Move camera target to the possesed agent.
        if (GameCamera.Instance != null)
        {
            GameCamera.Instance.TargetTransform = Agent == null ? null : Agent.transform;
        }
    }

    [Client]
    private void FindAgent()
    {
        if (!isServer)
        {
            if (Agent == null)
            {
                if (AgentID != 0)
                {
                    var go = ClientScene.FindLocalObject(new NetworkInstanceId(AgentID));
                    if (go != null)
                    {
                        var a = go.GetComponent<Agent>();
                        if (a != null)
                        {
                            // Found!
                            Agent = a;
                        }
                    }
                }
            }
            else
            {
                if (AgentID == 0)
                {
                    Agent = null;
                }
                else
                {
                    if (AgentID != Agent.netId.Value)
                    {
                        // We are set to the wrong agent, lets change that.
                        var go = ClientScene.FindLocalObject(new NetworkInstanceId(AgentID));
                        if (go != null)
                        {
                            var a = go.GetComponent<Agent>();
                            if (a != null)
                            {
                                // Found!
                                Agent = a;
                            }
                        }
                    }
                }
            }
        }
    }

    [Server]
    public void SetAgent(Agent a)
    {
        if(a == null)
        {
            AgentID = 0;
            Agent = null;
        }
        else
        {
            AgentID = a.netId.Value;
            Agent = a;
        }
    }
}