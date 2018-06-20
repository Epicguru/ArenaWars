using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [Tooltip("The initial speed of the projectile.")]
    public float Speed = 20f;

    [Tooltip("The maximum amount of bounces. Projectiles only bounce of non-penetratable objects.")]
    public float MaxBounces = 0;

    [Tooltip("The minimum and maximum angle change upon a bounce. Both values must be positive.")]
    public Vector2 BounceAngleChange = new Vector2(0f, 0f);

    private static Dictionary<string, ProjectileData> loaded;

    public static void LoadAll()
    {
        if (loaded != null)
            return;

        loaded = new Dictionary<string, ProjectileData>();
        var pds = Resources.LoadAll<ProjectileData>("Projectile Data");

        foreach(var pd in pds)
        {
            string name = pd.name.Trim();
            if (loaded.ContainsKey(name))
            {
                Debug.LogWarning("Duplicate name for loaded projectile data: '{0}'".Form(name));
            }
            else
            {
                loaded.Add(name, pd);
            }
        }

        Debug.Log("Loaded {0} projectile datas!".Form(loaded.Count));
    }

    public static void UnloadAll()
    {
        if (loaded == null)
            return;

        loaded.Clear();
        loaded = null;
    }

    public static bool IsLoaded(string id)
    {
        return loaded != null && loaded.ContainsKey(id);
    }

    public static ProjectileData Get(string id)
    {
        if (id == null)
            return null;

        if (!IsLoaded(id))
        {
            Debug.LogError("There is no loaded projectile data called '{0}'!".Form(id));
            return null;
        }

        return loaded[id];
    }
}