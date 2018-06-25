using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Region : MonoBehaviour
{
    // Is a part of a tile map, has its own texture.

    public const int PIXELS_PER_UNIT = 32;
    public const int CHUNK_SIZE = 16;
    public int X, Y;

    public bool Dirty
    {
        get
        {
            return _dirty;
        }
        private set
        {
            _dirty = value;
        }
    }
    [SerializeField]
    [ReadOnly]
    private bool _dirty;

    public MeshRenderer Renderer;

    [NonSerialized]
    private Texture2D texture;

    public Vector2Int GetRequiredTextureSize()
    {
        return new Vector2Int(CHUNK_SIZE * PIXELS_PER_UNIT, CHUNK_SIZE * PIXELS_PER_UNIT);
    }

    public void Update()
    {
        if (Dirty)
        {
            Apply();
        }

        // TEST - REMOVEME!
        MapIO.Update();
    }

    public void UponSpawn()
    {
        Dirty = false;
        SetupMesh();

        if (texture == null)
        {
            var size = GetRequiredTextureSize();
            texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, true, true);

            // TODO load in required stuff...
        }
        else
        {
            var size = GetRequiredTextureSize();
            if (texture.width != size.x || texture.height != size.y)
            {
                texture.Resize(size.x, size.y);
            }

            // TODO load in required stuff.
        }

        // Filter mode
        texture.filterMode = FilterMode.Bilinear;

        // Now ensure that it is applied to the material.
        SetRendererTexture();

        // Test tiles.
        var tile = TileData.Get(0);

        SetTilePixels(0, 0, tile.GetPixels(0));
        SetTilePixels(1, 1, tile.GetPixels(2));
        SetTilePixels(2, 1, tile.GetPixels(8));
    }

    public void UponDespawn()
    {
        if (texture != null)
        {
            texture = null;
        }

        Dirty = false;
    }

    public bool InRegionBounds(int x, int y)
    {
        return x >= 0 && x < CHUNK_SIZE && y >= 0 && y < CHUNK_SIZE;
    }

    public void SetTilePixels(int x, int y, Color32[] pixels)
    {
        if(!InRegionBounds(x, y))
        {
            Debug.LogError("The tile position ({0}, {1}) is out of region bounds. [{2}]".Form(x, y, this.ToString()));
            return;
        }

        if(texture == null)
        {
            Debug.LogError("The region texture is null, perhaps it has not be spawned? Cannot draw colours.");
            return;
        }

        if(pixels == null)
        {
            Debug.LogError("Cannot draw null pixel array to the region texture!");
            return;
        }

        const int TOTAL = PIXELS_PER_UNIT * PIXELS_PER_UNIT;
        if(pixels.Length != TOTAL)
        {
            Debug.LogError("The amount of pixels supplied ({0}) does not meet the requirement of exactly {1} pixels.".Form(pixels.Length, TOTAL));
            return;
        }

        texture.SetPixels32(x * PIXELS_PER_UNIT, y * PIXELS_PER_UNIT, PIXELS_PER_UNIT, PIXELS_PER_UNIT, pixels);

        Dirty = true;
    }

    private void Apply()
    {
        if (!Dirty)
        {
            return;
        }

        if(texture == null)
        {
            Debug.LogError("Cannot apply when the texture is null!");
            return;
        }

        SetRendererTexture();
        texture.Apply();
    }

    private void SetRendererTexture()
    {
        if(texture == null)
        {
            Debug.LogError("Tried to set renderer texture when texture is null!");
            return;
        }
        
        if(Renderer.material.mainTexture != texture)
        {
            Renderer.material.mainTexture = texture;
        }
    }

    private void SetupMesh()
    {
        Renderer.transform.localPosition = new Vector2(CHUNK_SIZE / 2f, CHUNK_SIZE / 2f);
        Renderer.transform.localScale = new Vector2(CHUNK_SIZE, CHUNK_SIZE);
    }

    public override string ToString()
    {
        return name + " ({0}, {1})";
    }
}