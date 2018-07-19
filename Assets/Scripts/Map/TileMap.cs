using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public const string METADATA_FILE = "Data.txt";
    public const string TILE_DATA_FILE = "Tile IDs.txt";
    public const string TILE_VARIATION_FILE = "Tile Variations.txt";

    public static TileMap Instance;

    public Transform RegionParent;
    public PoolableObject RegionPrefab;

    [System.NonSerialized] private Dictionary<int, Region> LoadedRegions = new Dictionary<int, Region>();
    [System.NonSerialized] private List<int> toLoad = new List<int>();
    [System.NonSerialized] private List<int> toBin = new List<int>();

    // Represents a loaded verison of a tile map, which can be played in.
    public TileMapData Data;
    [System.NonSerialized]
    public ushort[] TileIDs;
    [System.NonSerialized]
    public byte[] TileVariations;

    public string MapNameTemp = "Dev_0";

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TileIDs = new ushort[Data.SizeInTiles];
            TileVariations = new byte[Data.SizeInTiles];
        }

        if (IsLoaded())
        {
            // Unload and load in around around camera and all agents.
            var bounds = CameraBounds.Instance.RegionBounds;

            toLoad.Clear();
            toBin.Clear();

            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y <= bounds.yMax; y++)
                {
                    if(RegionInBounds(x, y))
                    {
                        int i = GetRegionIndex(x, y);
                        toLoad.Add(i);
                    }
                }
            }

            foreach (var pair in LoadedRegions)
            {
                if (!toLoad.Contains(pair.Key))
                {
                    // Is not requested to load this frame, goodbye!
                    toBin.Add(pair.Key);
                }
            }

            foreach (var index in toBin)
            {
                UnloadRegion(index);
            }

            foreach (var index in toLoad)
            {
                if (!IsRegionLoaded(index))
                {
                    LoadRegion(index);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            for (int i = 0; i < Data.SizeInRegions; i++)
            {
                if (IsRegionLoaded(i))
                    UnloadRegion(i);
            }
            Load(MapNameTemp, true);
        }

        if (IsLoaded() && Input.GetKeyDown(KeyCode.J))
        {
            Save(true, true);
        }

        Vector2Int mouseCoords = new Vector2Int((int)InputManager.MousePos.x, (int)InputManager.MousePos.y);
        if (Input.GetKey(KeyCode.Space) && IsLoaded() && this.TileInBounds(mouseCoords.x, mouseCoords.y))
        {
            int tileIndex = GetTileIndex(mouseCoords.x, mouseCoords.y);
            int tx = mouseCoords.x;
            int ty = mouseCoords.y;

            bool updated = false;
            if (Input.GetMouseButton(0))
            {
                TileIDs[tileIndex] = 1;
                updated = true;
            }
            if (Input.GetMouseButton(1))
            {
                TileIDs[tileIndex] = 0;
                updated = true;
            }

            if (updated)
            {
                TileVarResolver.UpdateChangedVariations(mouseCoords.x, mouseCoords.y, this);

                // Center tile
                DrawTile(tx, ty);

                // Surrounding tiles.
                DrawTile(tx - 1, ty);
                DrawTile(tx + 1, ty);
                DrawTile(tx, ty - 1);
                DrawTile(tx, ty + 1);
            }
        }
    }

    public bool IsSpotWalkable(int x, int y)
    {
        // It's only walkable if it's 0 (air).
        return TileInBounds(x, y) && TileIDs[GetTileIndex(x, y)] == 0;
    }

    public void DrawTile(int tileX, int tileY)
    {        
        if(TileInBounds(tileX, tileY))
        {
            int ri = GetRegionIndex(tileX / Region.SIZE, tileY / Region.SIZE);
            if (IsRegionLoaded(ri))
            {
                var r = GetSpawnedRegion(ri);
                int ox = tileX - (r.X * Region.SIZE);
                int oy = tileY - (r.Y * Region.SIZE);
                int ti = GetTileIndex(tileX, tileY);
                var tileID = TileIDs[ti];

                if (tileID != 0)
                {
                    r.SetTilePixels(ox, oy, TileData.Get(tileID).GetPixels(TileVariations[ti]));
                }
                else
                {
                    r.SetTilePixels(ox, oy, null);
                }
            }
        }
    }

    public Vector2Int GetRegionCoords(int tileX, int tileY)
    {
        return new Vector2Int(tileX / Region.SIZE, tileY / Region.SIZE);
    }

    public void LoadRegion(int regionIndex)
    {
        if (IsRegionLoaded(regionIndex))
        {
            Debug.LogError("Region @ index {0} is already spawned, cannot load it again!".Form(regionIndex));
        }
        else
        {
            var spawned = Pool.Get(RegionPrefab);
            var r = spawned.GetComponent<Region>();

            // Register.
            LoadedRegions.Add(regionIndex, r);

            // Give it index, X, Y values.
            var coords = GetRegionCoords(regionIndex);
            r.X = coords.x;
            r.Y = coords.y;
            r.Index = regionIndex;
            r.transform.position = new Vector2(r.X, r.Y) * Region.SIZE;

            DrawWholeRegion(r);

            r.PostSpawned();
        }
    }

    public void UnloadRegion(int regionIndex)
    {
        if (!IsRegionLoaded(regionIndex))
        {
            Debug.LogError("Region @ index {0} is not spawned, cannot unload it!".Form(regionIndex));
        }
        else
        {
            var region = LoadedRegions[regionIndex];
            Pool.Return(region.PoolableObject);

            // Unregister.
            LoadedRegions.Remove(regionIndex);
        }
    }

    public Region GetSpawnedRegion(int regionIndex)
    {
        if (IsRegionLoaded(regionIndex))
        {
            return LoadedRegions[regionIndex];
        }
        else
        {
            Debug.LogError("Region is not spawned, cannot access it!");
            return null;
        }
    }

    public Region GetSpawnedRegion(int regionX, int regionY)
    {
        int regionIndex = GetRegionIndex(regionX, regionY);
        if (IsRegionLoaded(regionIndex))
        {
            return LoadedRegions[regionIndex];
        }
        else
        {
            Debug.LogError("Region is not spawned, cannot access it!");
            return null;
        }
    }

    public int GetRegionIndex(int regionX, int regionY)
    {
        return regionX + regionY * Data.WidthInRegions;
    }

    public Vector2Int GetRegionCoords(int regionIndex)
    {
        return new Vector2Int(regionIndex % Data.WidthInRegions, regionIndex / Data.WidthInRegions);
    }

    public bool IsRegionLoaded(int regionIndex)
    {
        return LoadedRegions.ContainsKey(regionIndex);
    }

    public bool IsRegionSpawned(int regionX, int regionY)
    {
        return LoadedRegions.ContainsKey(GetRegionIndex(regionX, regionY));
    }  

    public void DrawWholeRegion(Region region)
    {
        if(region == null)
        {
            Debug.LogError("Region is null, cannot set pixels!");
            return;
        }

        if(region.Index == -1)
        {
            Debug.LogError("Region index is -1, meaning that it is invalid or uninitialized. Cannot set pixels!");
            return;
        }

        int size = Region.SQR_SIZE;
        int offset = size * region.Index;
        int subOff = 0;

        for (int x = 0; x < Region.SIZE; x++)
        {
            for (int y = 0; y < Region.SIZE; y++)
            {
                subOff = x + y * Region.SIZE;
                var id = TileIDs[offset + subOff];
                region.SetTilePixels(x, y, id == 0 ? null : TileData.Get(id).GetPixels(TileVariations[offset + subOff]));
            }
        }
    }

    public void Save(bool useTemp, bool deleteTemp = true)
    {
        if(Data == null)
        {
            Debug.LogError("Tile Map Data is null, cannot save! Invalid loaded map!");
            return;
        }

        // Should be in editor mode for this to work correctly, but it theoretically also works even at runtime.
        string zipFilePath = Data.SavedZipPath;
        Debug.Log("Saving map '{0}' to {1}".Form(this, zipFilePath));

        string fileSaveDir = null;
        if (useTemp)
        {
            // Save to a temporary folder.
            fileSaveDir = Path.Combine(Path.GetTempPath(), Data.InternalName);
        }
        else
        {
            // Save to the extracted (unzipped) folder, and then zip it up anyway.
            fileSaveDir = Path.Combine(GameIO.UnzippedMapsDirectory, Data.InternalName);
        }

        // Save all map data to file.
        Debug.Log("Saving files to '{0}' ({1}) ...".Form(fileSaveDir, useTemp ? "temp" : "pers"));
        GameIO.EnsureDirectory(fileSaveDir);
        var fs = MapIO.StartWrite(Path.Combine(fileSaveDir, TILE_DATA_FILE));
        MapIO.WriteTileIDs(fs, TileIDs);
        MapIO.End(fs);

        // Save metadata.
        GameIO.ObjectToFile(this.Data, Path.Combine(fileSaveDir, METADATA_FILE));

        // Work out all the tile varieties.
        // TODO
        fs = MapIO.StartWrite(Path.Combine(fileSaveDir, TILE_VARIATION_FILE));
        MapIO.WriteTileVariations(fs, this.TileVariations);
        MapIO.End(fs);

        // Grab all the saved files, and zip the up into the save zip path.
        MapIO.Zip(fileSaveDir, zipFilePath);

        // Delete the temporary directory, just to be clean.
        if (useTemp && deleteTemp)
        {
            Directory.Delete(fileSaveDir, true);
        }

        Debug.Log("Finished saving, zipped to '{0}'".Form(zipFilePath));        
    }

    public void Load(string mapInternalName, bool forceExtract)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        if(mapInternalName == null)
        {
            Debug.LogError("Map internal name is null, cannot load!");
            return;
        }

        string unzippedPath = GameIO.UnzippedMapsDirectory;
        if (!Directory.Exists(unzippedPath))
        {
            Debug.LogWarning("Unzipped map path ({0}) does not exist, creating it...".Form(unzippedPath));
            Directory.CreateDirectory(unzippedPath);
        }

        string zippedPath = GameIO.ZippedMapsDirectory;
        string zippedFilePath = Path.Combine(zippedPath, mapInternalName + ".zip");
        bool zippedExists = false;
        if (!Directory.Exists(zippedPath))
        {
            Debug.LogWarning("Zipped map path ({0}) does not exist, why? Map will not load unless it has already been unzipped previously.".Form(zippedPath));
        }
        else
        {
            if(File.Exists(zippedFilePath))
            {
                zippedExists = true;
            }
        }

        string destinationUnzippedDirectory = Path.Combine(unzippedPath, mapInternalName);

        if (forceExtract)
        {
            if (!zippedExists)
            {
                Debug.LogError("Extract is required (forced), but map zipped file '{0}' could not be found. Cannot load map!".Form(zippedPath));
                return;
            }

            // Extract the zipped file to the unzipped folder.
            MapIO.UnZip(zippedFilePath, destinationUnzippedDirectory, true);
            Debug.Log("Unzipped map '{0}' to {1}, was forced ({2}).".Form(mapInternalName, destinationUnzippedDirectory, Directory.Exists(destinationUnzippedDirectory) ? "not necessary" : "necessary"));
        }
        else
        {
            // Check to see if an unzipped version exists, otherwise extract...
            if (Directory.Exists(destinationUnzippedDirectory))
            {
                // It does exist! Assume that the files are all correct and loaded properly...
                Debug.Log("Extract not forced, unzipped version of '{0}' found.".Form(mapInternalName));
            }
            else
            {
                // An unzipped version does not exist, extract it!
                if (!zippedExists)
                {
                    Debug.LogError("Extract is required, but map zipped file '{0}' could not be found. Cannot load map!".Form(zippedPath));
                    return;
                }

                // Extract the zipped file to the unzipped folder.
                MapIO.UnZip(zippedFilePath, destinationUnzippedDirectory, true);
                Debug.Log("Unzipped map '{0}' to {1}, was NOT forced (necessary).".Form(mapInternalName, destinationUnzippedDirectory));
            }
        }

        // Now the extracted version should exist.

        // Load map metadata...
        this.Data = GameIO.FileToObject<TileMapData>(Path.Combine(destinationUnzippedDirectory, METADATA_FILE));

        // Load map tile ID data.
        string dataFile = Path.Combine(destinationUnzippedDirectory, TILE_DATA_FILE);
        var fs = MapIO.StartRead(dataFile);
        var data = MapIO.ReadAllTileIDs(fs, this.Data.SizeInRegions);
        this.TileIDs = null;
        this.TileIDs = data;
        MapIO.End(fs);

        // Load map tile variations...
        string variationFile = Path.Combine(destinationUnzippedDirectory, TILE_VARIATION_FILE);
        fs = MapIO.StartRead(variationFile);
        var varData = MapIO.ReadAllTileVariations(fs, this.Data.SizeInRegions);
        this.TileVariations = null;
        this.TileVariations = varData;
        MapIO.End(fs);

        // Run GC
        Debug.Log("Running GC...");
        Memory.GC();
        Debug.Log("Done!");

        // Done!
        sw.Stop();
        Debug.Log("Done loading map '{0}' in {1} seconds.".Form(mapInternalName, sw.Elapsed.TotalSeconds.ToString("N2")));
    }

    public int GetTileIndex(int x, int y)
    {
        int regionX = x / Region.SIZE;
        int regionY = y / Region.SIZE;

        int regionIndex = GetRegionIndex(regionX, regionY);
        int startIndex = regionIndex * Region.SQR_SIZE;

        int startX = regionX * Region.SIZE;
        int startY = regionY * Region.SIZE;

        int offX = x - startX;
        int offY = y - startY;

        int regionTileIndexOffset = offX + offY * Region.SIZE;

        return startIndex + regionTileIndexOffset;
    }

    public bool RegionInBounds(int rx, int ry)
    {
        if (rx < 0 || rx >= Data.WidthInRegions || ry < 0 || ry >= Data.HeightInRegions)
        {
            return false;
        }
        return true;
    }

    public bool TileInBounds(int tx, int ty)
    {
        if (tx < 0 || tx >= Data.WidthInTiles || ty < 0 || ty >= Data.HeightInTiles)
        {
            return false;
        }
        return true;
    }

    public bool IsLoaded()
    {
        return Data != null && TileIDs != null;
    }

    public override string ToString()
    {
        return "Tile Map " + (Data == null ? "INVALID" : "- " + Data.InternalName);
    } 
}