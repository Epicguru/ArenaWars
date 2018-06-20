using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Agent))]
public class AgentHands : NetworkBehaviour
{
    public NetParent HandsParent;

    public Agent Agent
    {
        get
        {
            if(_agent == null)
            {
                _agent = GetComponent<Agent>();
            }
            return _agent;
        }
    }
    private Agent _agent;

    public void Awake()
    {
        if(HandsParent == null)
        {
            Debug.LogError("Hands NetParent value not set - it is required to put items in the hands of the agent! ({0})".Form(name));
        }
    }

    public Item GetHeldItem()
    {
        if (HandsParent.Children.Count == 0)
            return null;

        var first = HandsParent.Children[0];

        if (first == null)
            return null;

        var item = first.GetComponent<Item>();

        return item;
    }

    public void Update()
    {
        var held = GetHeldItem();
        if (held != null)
        {
            held.transform.localPosition = Vector3.zero;
        }
    }
}