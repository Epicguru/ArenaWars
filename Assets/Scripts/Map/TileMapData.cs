using Newtonsoft.Json;
using System.IO;
using UnityEngine;

[System.Serializable]
public class TileMapData
{
    public string DisplayName = "Default Name";
    public string InternalName = "Default_Name";
    public string Description = "It's a map that you can play in.";
    public int WidthInChunks = 2, HeightInChunks = 2;

    public int SizeInTiles
    {
        get
        {
            return WidthInChunks * HeightInChunks * Region.SQR_CHUNK_SIZE;
        }
    }

    public string SavedZipPath
    {
        get
        {
            return Path.Combine(GameIO.ZippedMapsDirectory, InternalName + ".zip");
        }
    }

    public string SavedUnzippedPath
    {
        get
        {
            return Path.Combine(GameIO.UnzippedMapsDirectory, InternalName);
        }
    }

    public string ToJson()
    {
        return GameIO.ObjectToJson(this, Formatting.Indented, true);
    }
}