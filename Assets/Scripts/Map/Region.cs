using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    // Is a part of a tile map, has its own texture.

    public const int PIXELS_PER_UNIT = 32;
    public RectInt Bounds;

    public int X
    {
        get
        {
            return Bounds.xMin;
        }
        set
        {
            Bounds.xMin = value;
        }
    }
    public int Y
    {
        get
        {
            return Bounds.yMin;
        }
        set
        {
            Bounds.yMin = value;
        }
    }
    public int Width
    {
        get
        {
            return Bounds.width;
        }
        set
        {
            Bounds.width = value;
        }
    }
    public int Height
    {
        get
        {
            return Bounds.height;
        }
        set
        {
            Bounds.height = value;
        }
    }

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

    private Texture2D texture;

    public Vector2Int GetRequiredTextureSize()
    {
        return new Vector2Int(Width * PIXELS_PER_UNIT, Height * PIXELS_PER_UNIT);
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
        SetupMesh();

        if (texture == null)
        {
            var size = GetRequiredTextureSize();
            texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);

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

        // Now ensure that it is applied to the material.
        SetRendererTexture();
    }

    public void UponDespawn()
    {
        if(texture != null)
        {
            texture = null;
        }

        Dirty = false;
    }

    public bool InRegionBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public void SetColours(int x, int y, Color32[] colours)
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

        if(colours == null)
        {
            Debug.LogError("Cannot draw null colour array to the region texture!");
            return;
        }

        int pixels = PIXELS_PER_UNIT * PIXELS_PER_UNIT;
        if(colours.Length != pixels)
        {
            Debug.LogError("The amount of pixels supplied ({0}) does not meet the requirement of exactly {1} pixels.".Form(colours.Length, pixels));
            return;
        }

        int X = x * PIXELS_PER_UNIT;
        int Y = y * PIXELS_PER_UNIT;

        texture.SetPixels32(X, Y, PIXELS_PER_UNIT, PIXELS_PER_UNIT, colours);

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
        Renderer.transform.localPosition = new Vector2(Width / 2f, Height / 2f);
        Renderer.transform.localScale = new Vector2(Width, Height);
    }

    public override string ToString()
    {
        return name + " " + Bounds;
    }
}