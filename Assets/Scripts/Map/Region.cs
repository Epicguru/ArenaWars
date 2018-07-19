using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(PoolableObject))]
[RequireComponent(typeof(RegionPhysics))]
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

    public RegionPhysics RegionPhysics
    {
        get
        {
            if(_regionPhysics == null)
            {
                _regionPhysics = GetComponent<RegionPhysics>();
            }
            return _regionPhysics;
        }
    }
    private RegionPhysics _regionPhysics;

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
    public SpriteRenderer Hider;

    [NonSerialized]
    private Texture2D texture;
    private bool initialApplication;

    private ScheduledJob TexJob;

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

    public void OnDestroy()
    {
        if(TexJob != null)
        {
            TexJob.State = JobState.CANCELLED;
        }

        if(texture != null)
        {
            texture = null;
            Memory.UnloadUnusedAssets();
        }
    }

    public void UponSpawn()
    {
        Dirty = false;
        Index = -1;
        X = 0;
        Y = 0;

        // Hide until the texture is loaded!
        initialApplication = true;
        Hider.gameObject.SetActive(true);
        var c = Hider.color;
        c.a = 1f;
        Hider.color = c;

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

    private void UponTextureApplied()
    {
        if (initialApplication)
        {
            initialApplication = false;

            // Remove the hider...
            StartCoroutine(HideTheHider());
        }
    }

    private IEnumerator HideTheHider()
    {
        if (!Hider.gameObject.activeSelf)
        {
            yield break;
        }

        const int STEPS = 10;
        const float TARGET_TIME = 0.1f;
        for (int i = 0; i < STEPS; i++)
        {
            var c = Hider.color;
            c.a -= 1f / STEPS;
            Hider.color = c;
            yield return new WaitForSecondsRealtime(TARGET_TIME / STEPS);
        }

        Hider.gameObject.SetActive(false);
    }

    public void UponDespawn()
    {
        Dirty = false;
        StopAllCoroutines();

        // Cancel any pending texture job.
        if(TexJob != null)
        {
            if(TexJob.State == JobState.PENDING)
            {
                TexJob.State = JobState.CANCELLED;
            }
        }

        // Physics...
        RegionPhysics.Despawn();
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

        if(TexJob == null)
        {
            TexJob = new ScheduledJob();
        }
        else
        {
            if(TexJob.State == JobState.PENDING)
            {
                Dirty = false;
                return;
            }
        }
        TexJob.State = JobState.IDLE;
        TexJob.Action = texture.Apply;
        TexJob.UponCompletion = UponTextureApplied;

        SetRendererTexture();
        Scheduler.AddJob(TexJob);
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

    public void PostSpawned()
    {
        // Called after UponSpawn, once everything is set up correctly (apart from the texture and physics)

        // Physics...
        RegionPhysics.Despawn(); // Clear any old ones...
        RegionPhysics.Build();
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