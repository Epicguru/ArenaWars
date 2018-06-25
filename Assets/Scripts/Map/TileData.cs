using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "Tile Data")]
public class TileData : ScriptableObject
{
    private static Dictionary<ushort, TileData> loaded;
    private static Vector2Int FullTextureSize;

    [Tooltip("The unique, non changeable ID value.")]
    public ushort ID;

    public Sprite[] Variations;

    [NonSerialized]
    private Color32[] FullTexture;
    [NonSerialized]
    private Color32[][] Cache; 

    public Color32[] GetPixels(int index)
    {
        if(index < 0 || index >= Variations.Length)
        {
            Debug.LogError("Index out of bounds for pixel request: {0}. Must be >= 0 and < {1}".Form(index, Variations.Length));
            return null;
        }

        if(Cache == null)
        {
            Cache = new Color32[Variations.Length][];
        }

        if(Cache[index] == null)
        {
            // Load and cache...
            var sprite = Variations[index];
            if(sprite == null)
            {
                Debug.LogError("Sprite for index {0} in tile '{1}' is null! Cannot extract pixel colours!".Form(index, this.name));
                return null;
            }

            // Load full texture if it is null...
            if(FullTexture == null)
            {
                FullTexture = sprite.texture.GetPixels32();
                FullTextureSize = new Vector2Int(sprite.texture.width, sprite.texture.height);
            }

            // Get sprite bounds within packed texture.
            var bounds = sprite.textureRect;

            // Get pixels from the loaded texture pixels.
            // Rows, one after another.
            int width = (int)bounds.width;
            int height = (int)bounds.height;
            int startX = (int)bounds.xMin;
            int startY = (int)bounds.yMin;

            Cache[index] = new Color32[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = (startX + x) + ((y + startY) * FullTextureSize.x);
                    var pixel = FullTexture[i];
                    int j = x + y * width;
                    Cache[index][j] = pixel;
                }
            }

            // Return freshly cached pixels.
            return Cache[index];
        }
        else
        {
            // Return cached value.
            return Cache[index];
        }
    }

    public void ClearCache()
    {
        Cache = null;
    }

    public static bool IsLoaded(ushort id)
    {
        return loaded != null && loaded.ContainsKey(id);
    }

    public static TileData Get(ushort id)
    {
        if (!IsLoaded(id))
        {
            Debug.LogError("Tile for ID {0} is not loaded, returning null.".Form(id));
            return null;
        }

        return loaded[id];
    }

    public static void LoadAll()
    {
        if (loaded != null)
            return;

        loaded = new Dictionary<ushort, TileData>();
        var l = Resources.LoadAll<TileData>("Tiles");

        foreach(var t in l)
        {
            if (loaded.ContainsKey(t.ID))
            {
                Debug.LogError("Tile is already loaded for ID {0}! Tile '{1}' and '{2}' have this same conflicting ID's.".Form(t.ID, t.name, loaded[t.ID].name));
            }
            else
            {
                loaded.Add(t.ID, t);
            }
        }
    }   

    public static void UnloadAll()
    {
        if(loaded != null)
        {
            loaded.Clear();
            loaded = null;
        }
    }
}