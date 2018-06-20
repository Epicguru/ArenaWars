using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Tooltip("The unique, non changeable ID value.")]
    public ushort ID;

    public string Name = "Tile Name";

    public Sprite[] Variations;

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
                Debug.LogError("Sprite for index {0} in tile '{1}' is null! Cannot extract pixel colours!".Form(index, Name));
                return null;
            }

            // Get sprite bounds within packed texture.
            var bounds = sprite.textureRect;

            // Get pixels based on bounds.
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
}