using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Item))]
public abstract class Weapon : NetworkBehaviour
{
    // It's an item, which is a weapon, and therefore can be used to deal damage.
    // Could be melee or ranged: other classes define concrete behaviour.

    public Item Item
    {
        get
        {
            if(_item == null)
            {
                _item = GetComponent<Item>();
            }
            return _item;
        }
    }
    private Item _item;

    // Only valid on server!
    [Server]
    public abstract void StartAttack();

    [Server]
    public abstract void EndAttack();
}