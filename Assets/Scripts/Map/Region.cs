using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(PoolableObject))]
public class Region : MonoBehaviour
{
    // Is a part of a tile map, has its own texture.
    public const int PIXELS_PER_UNIT = 32;
    public const int SIZE = 16;
    public const int SQR_SIZE = SIZE * SIZE;
    public const int SQR_PIXELS_PER_UNIT = PIXELS_PER_UNIT * PIXELS_PER_UNIT;

    public static Color32[] BLANK_TILE = new Color32[SQR_PIXELS_PER_UNIT];

    public int X, Y;
    public int Index = -1;

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
    public PoolableObject PoolableObject;

    [NonSerialized]
    private Texture2D texture;

    public Vector2Int GetRequiredTextureSize()
    {
        return new Vector2Int(SIZE * PIXELS_PER_UNIT, SIZE * PIXELS_PER_UNIT);
    }

    public void Update()
    {
        if (Dirty)
        {
            Apply();
        }
    }

    public void UponSpawn()
    {
        Dirty = false;
        Index = -1;
        X = 0;
        Y = 0;

        SetupMesh();

        if (texture == null)
        {
            var size = GetRequiredTextureSize();
            texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, true, true);
        }
        else
        {
            var size = GetRequiredTextureSize();
            if (texture.width != size.x || texture.height != size.y)
            {
                texture.Resize(size.x, size.y);
            }
        }

        // Filter mode
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        // Now ensure that it is applied to the material.
        SetRendererTexture();
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
        return x >= 0 && x < SIZE && y >= 0 && y < SIZE;
    }

    public void SetTilePixels(int x, int y, Color32[] pixels)
    {
        if (!InRegionBounds(x, y))
        {
            Debug.LogError("The tile position ({0}, {1}) is out of region bounds. [{2}]".Form(x, y, this.ToString()));
            return;
        }

        if (texture == null)
        {
            Debug.LogError("The region texture is null, perhaps it has not be spawned? Cannot draw colours.");
            return;
        }

        const int TOTAL = PIXELS_PER_UNIT * PIXELS_PER_UNIT;
        if (pixels != null && pixels.Length != TOTAL)
        {
            Debug.LogError("The amount of pixels supplied ({0}) does not meet the requirement of exactly {1} pixels.".Form(pixels.Length, TOTAL));
            return;
        }

        texture.SetPixels32(x * PIXELS_PER_UNIT, y * PIXELS_PER_UNIT, PIXELS_PER_UNIT, PIXELS_PER_UNIT, pixels == null ? BLANK_TILE : pixels);

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

        Dirty = false;
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
        Renderer.transform.localPosition = new Vector2(SIZE / 2f, SIZE / 2f);
        Renderer.transform.localScale = new Vector2(SIZE, SIZE);
    }

    public override string ToString()
    {
        return name + " ({0}, {1})";
    }
}