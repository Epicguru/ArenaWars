
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetParent : MonoBehaviour
{
    [SerializeField]
    private byte ID;

    public NetworkIdentity NetID
    {
        get
        {
            if(_netID == null)
            {
                _netID = GetComponentInParent<NetworkIdentity>();
            }
            return _netID;
        }
    }
    private NetworkIdentity _netID;

    public List<NetParenting> Children = new List<NetParenting>();

    public void Awake()
    {
        if (!IsValid())
        {
            Debug.LogError("Invalid NetParent! '{0}' is not configured correctly: it must have a parent with a NetworkIdentity.");
            return;
        }
    }

    public byte GetID()
    {
        return this.ID;
    }

    public bool IsValid()
    {
        return NetID != null;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}