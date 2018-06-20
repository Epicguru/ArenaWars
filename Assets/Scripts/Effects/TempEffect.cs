using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PoolableObject))]
public class TempEffect : MonoBehaviour
{
    private static Dictionary<TempEffects, TempEffect> loaded;

    // Describes a temporary one-shot style particle effect that can be easily instanciated through a pooling system.
    public TempEffects ID;
    public float Duration = 2f;

    public ParticleSystem Particles;

    [Header("Audio")]
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 0.5f;

    [Header("Animation")]
    public Animator Anim;

    public PoolableObject PoolableObject
    {
        get
        {
            if(_po == null)
            {
                _po = GetComponent<PoolableObject>();
            }
            return _po;
        }
    }
    private PoolableObject _po;

    private float timer = 0f;
    private bool playAudio;

    public void Spawn()
    {
        playAudio = true;
        Play();

        if (Anim != null)
        {
            Anim.StopPlayback();
            Anim.StartPlayback();
        }
    }

    public void Play()
    {
        if (Particles != null)
        {
            if (Particles.isPlaying)
            {
                Particles.Stop(true);
            }
            Particles.Play(true);
        }

        timer = 0f;
    }

    public void Update()
    {
        if (playAudio)
        {
            playAudio = false;
            if(Clip != null)
            {
                AudioSource.PlayClipAtPoint(Clip, transform.position, Volume);
            }
        }
        timer += Time.deltaTime;

        if(timer >= Duration)
        {
            if(Particles != null)
                Particles.Stop(true);

            // Return to pool.
            Pool.Return(GetComponent<PoolableObject>());
        }
    }

    public static void LoadAll()
    {
        if (loaded != null)
            return;

        loaded = new Dictionary<TempEffects, TempEffect>();
        var l = Resources.LoadAll<TempEffect>("Effects");
        for (int i = 0; i < l.Length; i++)
        {
            var effect = l[i];

            if (loaded.ContainsKey(effect.ID))
            {
                Debug.LogError("Duplicate effect ID: {0}! Will not be added.".Form(effect.ID));
            }
            else
            {
                loaded.Add(effect.ID, effect);
            }
        }
    }

    public static void UnloadAll()
    {
        if (loaded == null)
            return;

        loaded.Clear();
        loaded = null;
    }

    public static bool IsEffectLoaded(TempEffects ID)
    {
        return loaded != null && loaded.ContainsKey(ID);
    }

    public static TempEffect Get(TempEffects id)
    {
        if (!IsEffectLoaded(id))
        {
            Debug.LogError("Effect {0} is not loaded; Are any effects loaded: {1}".Form(id, loaded != null));
            return null;
        }

        return loaded[id];
    }

    public static GameObject Spawn(TempEffects id)
    {
        var prefab = Get(id);
        if (prefab == null)
            return null;

        var spawned = Pool.Get(prefab.PoolableObject);

        return spawned;
    }

    /// <summary>
    /// Server only! Spawns a temp effect across all clients and on the server. Does not use networked objects,
    /// so is quite efficient. Just one RPC, with around 16 bytes per call.
    /// </summary>
    /// <returns></returns>
    public static GameObject NetSpawn(TempEffects id, Vector2 position, float angle)
    {
        if (!NetworkServer.active)
            return null;

        var spawned = Spawn(id);
        if (spawned == null)
            return null;

        Spawnables.Instance.RpcSpawnTempEffect((byte)id, position, angle);

        spawned.transform.position = position;
        spawned.transform.eulerAngles = new Vector3(0f, 0f, angle);
        return spawned;
    }

    public static void NetCallback(TempEffects id, Vector2 position, float angle)
    {
        var spawned = Spawn(id);

        if (spawned == null)
            return;

        spawned.transform.position = position;
        spawned.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}

public enum TempEffects : byte
{
    HIT_SPARKS,
    HIT_SPARKS_SMALL
}