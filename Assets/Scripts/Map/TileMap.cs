using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public const string METADATA_FILE = "Data.txt";
    public const string TILE_DATA_FILE = "Tile Data.txt";

    public Transform RegionParent;
    public PoolableObject RegionPrefab;

    private Dictionary<int, Region> SpawnedRegions = new Dictionary<int, Region>();

    // Represents a loaded verison of a tile map, which can be played in.
    public TileMapData Data;
    [System.NonSerialized]
    public ushort[] MapData;

    public string MapNameTemp = "Dev_0";

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            MapData = new ushort[Data.SizeInTiles];
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                // Load 4 regions.
                LoadRegion(GetRegionIndex(0, 0));
                LoadRegion(GetRegionIndex(1, 0));
                LoadRegion(GetRegionIndex(0, 1));
                LoadRegion(GetRegionIndex(1, 1));
            }
            else
            {
                // Unload all 4 regions.
                UnloadRegion(GetRegionIndex(0, 0));
                UnloadRegion(GetRegionIndex(1, 0));
                UnloadRegion(GetRegionIndex(0, 1));
                UnloadRegion(GetRegionIndex(1, 1));
            }

        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Load(MapNameTemp, true);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Save(true, true);
        }

        Vector2Int mouseCoords = new Vector2Int((int)InputManager.MousePos.x, (int)InputManager.MousePos.y);
        var rc = GetRegionCoords(mouseCoords.x, mouseCoords.y);
        int ri = GetRegionIndex(rc.x, rc.y);
        Vector2Int regionStart = new Vector2Int(rc.x * Region.CHUNK_SIZE, rc.y * Region.CHUNK_SIZE);
        var regionOffset = mouseCoords - regionStart;
        int offset = Region.SQR_CHUNK_SIZE * ri;
        int subOff = regionOffset.x + regionOffset.y * Region.CHUNK_SIZE;
        int index = offset + subOff;

        if (Input.GetMouseButtonDown(0))
        {
            MapData[index] = 1;
            GetSpawnedRegion(ri).SetTilePixels(regionOffset.x, regionOffset.y, TileData.Get(1).GetPixels(0));
        }
        if (Input.GetMouseButtonDown(1))
        {
            MapData[index] = 0;
        }
    }

    public Vector2Int GetRegionCoords(int tileX, int tileY)
    {
        return new Vector2Int(tileX / Region.CHUNK_SIZE, tileY / Region.CHUNK_SIZE);
    }

    public void LoadRegion(int regionIndex)
    {
        if (IsRegionSpawned(regionIndex))
        {
            Debug.LogError("Region @ index {0} is already spawned, cannot load it again!".Form(regionIndex));
        }
        else
        {
            var spawned = Pool.Get(RegionPrefab);
            var r = spawned.GetComponent<Region>();

            // Register.
            SpawnedRegions.Add(regionIndex, r);

            // Give it index, X, Y values.
            var coords = GetRegionCoords(regionIndex);
            r.X = coords.x;
            r.Y = coords.y;
            r.Index = regionIndex;
            r.transform.position = new Vector2(r.X, r.Y) * Region.CHUNK_SIZE;
            SetAllRegionPixels(r);
        }
    }

    public void UnloadRegion(int regionIndex)
    {
        if (!IsRegionSpawned(regionIndex))
        {
            Debug.LogError("Region @ index {0} is not spawned, cannot unload it!".Form(regionIndex));
        }
        else
        {
            var region = SpawnedRegions[regionIndex];
            Pool.Return(region.PoolableObject);

            // Unregister.
            SpawnedRegions.Remove(regionIndex);
        }
    }

    public Region GetSpawnedRegion(int regionIndex)
    {
        if (IsRegionSpawned(regionIndex))
        {
            return SpawnedRegions[regionIndex];
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
        if (IsRegionSpawned(regionIndex))
        {
            return SpawnedRegions[regionIndex];
        }
        else
        {
            Debug.LogError("Region is not spawned, cannot access it!");
            return null;
        }
    }

    public Vector2Int GetRegionCoords(int regionIndex)
    {
        return new Vector2Int(regionIndex % Data.WidthInChunks, regionIndex / Data.HeightInChunks);
    }

    public bool IsRegionSpawned(int regionIndex)
    {
        return SpawnedRegions.ContainsKey(regionIndex);
    }

    public bool IsRegionSpawned(int regionX, int regionY)
    {
        return SpawnedRegions.ContainsKey(GetRegionIndex(regionX, regionY));
    }

    public int GetRegionIndex(int regionX, int regionY)
    {
        return regionX + regionY * Data.WidthInChunks;
    }

    public void SetAllRegionPixels(Region region)
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

        int size = Region.SQR_CHUNK_SIZE;
        int offset = size * region.Index;
        int subOff = 0;

        for (int x = 0; x < Region.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Region.CHUNK_SIZE; y++)
            {
                subOff = x + y * Region.CHUNK_SIZE;
                var id = MapData[offset + subOff];

                if (id == 0)
                    continue;

                // TODO How to save and load indexes?
                region.SetTilePixels(x, y, TileData.Get(id).GetPixels(0));
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
        MapIO.WriteValues(fs, MapData);
        MapIO.End(fs);

        // Save metadata.
        GameIO.ObjectToFile(this.Data, Path.Combine(fileSaveDir, METADATA_FILE));

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

        // Load map data...
        this.Data = GameIO.FileToObject<TileMapData>(Path.Combine(destinationUnzippedDirectory, METADATA_FILE));

        // Load map data.
        string dataFile = Path.Combine(destinationUnzippedDirectory, TILE_DATA_FILE);
        var fs = MapIO.StartRead(dataFile);
        var data = MapIO.ReadAll(fs, this.Data.WidthInChunks * this.Data.HeightInChunks);
        this.MapData = data;
        MapIO.End(fs);

        // Done!
        sw.Stop();
        Debug.Log("Done loading map '{0}' in {1} seconds.".Form(mapInternalName, sw.Elapsed.TotalSeconds.ToString("N2")));
    }

    public override string ToString()
    {
        return "Tile Map " + (Data == null ? "INVALID" : "- " + Data.InternalName);
    }
}