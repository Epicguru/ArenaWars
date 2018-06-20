using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetParenting : NetworkBehaviour
{
    // Used to syncronise children across the network.
    // Intended for use with instantiated objects only.
    // Does not change position, only parenting.

    [SyncVar(hook = "ParentChanged")]
    [ReadOnly]
    [SerializeField]
    private uint parentID;

    [SyncVar]
    [ReadOnly]
    [SerializeField]
    private byte subParentID;

    public NetParent CurrentParent
    {
        get;
        private set;
    }

    public override void OnStartClient()
    {
        ParentChanged(parentID);
    }

    public void SetParent(NetParent parent, bool earlyBypass = false)
    {
        if (!isServer && !earlyBypass)
        {
            Debug.LogError("Can only change parenting on server!");
            return;
        }

        if(parent == null)
        {
            if(transform.parent == null)
            {
                // Do nothing, no change.
                return;
            }
            else
            {
                // Set parent to null, and update state.
                transform.SetParent(null);

                // No need to change sub parent ID...
                // But the real parent ID is set to 0 (invalid)
                parentID = 0;

                if(CurrentParent != null)
                {
                    // Tell the parent that we are leaving...
                    if(CurrentParent.Children.Contains(this))
                        CurrentParent.Children.Remove(this);
                }
                CurrentParent = null;
            }
        }
        else
        {
            // The following line is the pinnacle of programming.
            if(parent.transform == transform.parent)
            {
                // Do nothing, no change.
                return;
            }
            else
            {
                // Check if it is valid...
                if (!parent.IsValid())
                {
                    Debug.LogWarning("Invalid NetParent setup, will not change parenting. Make sure that the NetParent has a parent that has a NetworkIdentity.");
                    return;
                }

                // Set the parent here on the server, and update state.
                transform.SetParent(parent.transform);

                CurrentParent = parent;
                parent.Children.Add(this);

                // Set sub parent ID and then parent ID, in that order.
                subParentID = parent.GetID();
                parentID = parent.NetID.netId.Value;
            }
        }
    }

    private void ParentChanged(uint newID)
    {
        parentID = newID;

        // Do not change anything if on server...
        if (isServer)
            return;

        // If the ID is zero, then we have unparented...
        if(newID == 0)
        {
            transform.SetParent(null);
            if (CurrentParent != null)
            {
                if (CurrentParent.Children.Contains(this))
                    CurrentParent.Children.Remove(this);
            }
            CurrentParent = null;
            return;
        }

        // When the parent changes, the sub-parent has changed too.
        var obj = ClientScene.FindLocalObject(new NetworkInstanceId(newID));

        if(obj == null)
        {
            Debug.LogError("Networked parenting failed: This ({0}) failed to find client scene object with net instance ID {1}. It should be instanciated on server and clients.".Form(name, newID));
            return;
        }

        // Get the sub parents, called NetParents.
        var hooks = obj.GetComponentsInChildren<NetParent>();

        if(hooks == null || hooks.Length == 0)
        {
            Debug.LogError("Network parenting failed: This ({0}) failed to find any NetParent components on '{1}' or any of its children. Looking for hook ID {2}.".Form(name, obj.name, subParentID));
            return;
        }

        bool parented = false;
        for (int i = 0; i < hooks.Length; i++)
        {
            if(hooks[i].GetID() == subParentID)
            {
                transform.SetParent(hooks[i].transform);
                parented = true;

                CurrentParent = hooks[i];
                hooks[i].Children.Add(this);
                break;
            }
        }

        if (!parented)
        {
            Debug.LogError("Network parenting failed: This ({0}) failed to find a NetParent of ID {1}, although there were {2} NetParents.".Form(name, subParentID, hooks.Length));
            return;
        }
    }

    public void OnDestroy()
    {
        if (CurrentParent != null)
        {
            // Tell the parent that we are leaving...
            if (CurrentParent.Children.Contains(this))
                CurrentParent.Children.Remove(this);
        }
    }
}