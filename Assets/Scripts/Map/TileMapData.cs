using Newtonsoft.Json;
using System.IO;
using UnityEngine;

[System.Serializable]
public class TileMapData
{
    public string DisplayName = "Default Name";
    public string InternalName = "Default_Name";
    public string Description = "It's a map that you can play in.";
    public int WidthInRegions = 2, HeightInRegions = 2;

    public int WidthInTiles
    {
        get
        {
            return WidthInRegions * Region.SIZE;
        }
    }

    public int HeightInTiles
    {
        get
        {
            return HeightInRegions * Region.SIZE;
        }
    }

    public int SizeInRegions
    {
        get
        {
            return WidthInRegions * HeightInRegions;
        }
    }

    public int SizeInTiles
    {
        get
        {
            return SizeInRegions * Region.SQR_SIZE;
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