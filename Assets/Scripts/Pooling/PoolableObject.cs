using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoolableObject : MonoBehaviour
{
    public int PrefabID { get; private set; }

    [Tooltip("Is this a UI prefab?")]
    public bool IsUI = false;

    public UnityEvent OnSpawn = new UnityEvent();
    public UnityEvent OnDespawn = new UnityEvent();

    public virtual void Spawn(Vector3 position, Quaternion rotation, Transform parent)
    {
        gameObject.SetActive(true);
        transform.position = position;
        transform.rotation = rotation;
        transform.SetParent(parent, !IsUI);

        if(OnSpawn != null)
        {
            OnSpawn.Invoke();
        }
    }

    public virtual void Despawn()
    {
        if(OnDespawn != null)
        {
            OnDespawn.Invoke();
        }
    }

    public virtual void Setup(int prefabID)
    {
        this.PrefabID = prefabID;
    }
}