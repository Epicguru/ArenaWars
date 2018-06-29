
using System.Collections.Generic;
using UnityEngine;

public static class TileVarResolver
{
    private static bool[] surroundings = new bool[4];

    public static byte GetVariation(bool l, bool r, bool d, bool u)
    {
        byte index = 0;
        if (d)
        {
            index += 1;
        }
        if (r)
        {
            index += 2;
        }
        if (u)
        {
            index += 4;
        }
        if (l)
        {
            index += 8;
        }
        return index;
    }

    public static bool[] GetSurroundings(int x, int y, TileMap map)
    {
        if (map == null || !map.IsLoaded())
        {
            Debug.LogError("Map is null or unloaded!");
            return null;
        }

        if(!map.TileInBounds(x, y))
        {
            Debug.LogError("Tile @ {0}, {1} is out of map bounds!".Form(x, y));
            return null;
        }

        bool left = false;
        if (map.TileInBounds(x - 1, y))
        {
            left = map.TileIDs[map.GetTileIndex(x - 1, y)] != 0;
        }

        bool right = false;
        if (map.TileInBounds(x + 1, y))
        {
            right = map.TileIDs[map.GetTileIndex(x + 1, y)] != 0;
        }

        bool down = false;
        if (map.TileInBounds(x, y - 1))
        {
            down = map.TileIDs[map.GetTileIndex(x, y - 1)] != 0;
        }

        bool up = false;
        if (map.TileInBounds(x, y + 1))
        {
            up = map.TileIDs[map.GetTileIndex(x, y + 1)] != 0;
        }

        surroundings[0] = left;
        surroundings[1] = right;
        surroundings[2] = down;
        surroundings[3] = up;

        return surroundings;
    }

    public static byte GetVariation(int x, int y, TileMap map)
    {
        if (map == null || !map.IsLoaded())
        {
            Debug.LogError("Map is null or unloaded!");
            return 0;
        }

        if (!map.TileInBounds(x, y))
        {
            Debug.LogError("Tile X or Y value out of range: {0}, {1} with width {2} and height {3} in tiles.".Form(x, y, map.Data.WidthInTiles, map.Data.HeightInTiles));
            return 0;
        }

        var s = GetSurroundings(x, y, map);
        byte index = GetVariation(s[0], s[1], s[2], s[3]);

        return index;
    }

    public static void UpdateAllVariations(TileMap map)
    {
        if (map == null || !map.IsLoaded())
        {
            Debug.LogError("Map is null or unloaded!");
            return;
        }

        byte[] bytes = null;
        if(map.TileVariations == null || map.TileVariations.Length != map.Data.SizeInTiles)
        {
            bytes = new byte[map.Data.SizeInTiles];
        }
        else
        {
            bytes = map.TileVariations;
        }

        for (int x = 0; x < map.Data.WidthInTiles; x++)
        {
            for (int y = 0; y < map.Data.HeightInTiles; y++)
            {
                int index = map.GetTileIndex(x, y);
                bytes[index] = GetVariation(x, y, map);
            }
        }

        map.TileVariations = bytes;
    }

    public static void UpdateChangedVariations(int x, int y, TileMap map)
    {
        if (map == null || !map.IsLoaded())
        {
            Debug.LogError("Map is null or unloaded!");
            return;
        }

        if (map.TileVariations == null)
        {
            Debug.LogError("Tile variations array is null, cannot update!");
            return;
        }

        if(!map.TileInBounds(x, y))
        {
            Debug.LogError("Tile out of bounds, cannot update surrounding variations.");
            return;
        }

        // Center
        int index = map.GetTileIndex(x, y);
        map.TileVariations[index] = GetVariation(x, y, map);

        // Left
        if(map.TileInBounds(x - 1, y))
        {
            index = map.GetTileIndex(x - 1, y);
            map.TileVariations[index] = GetVariation(x - 1, y, map);
        }

        // Right
        if (map.TileInBounds(x + 1, y))
        {
            index = map.GetTileIndex(x + 1, y);
            map.TileVariations[index] = GetVariation(x + 1, y, map);
        }

        // Down
        if (map.TileInBounds(x, y - 1))
        {
            index = map.GetTileIndex(x, y - 1);
            map.TileVariations[index] = GetVariation(x, y - 1, map);
        }

        // Up
        if (map.TileInBounds(x, y + 1))
        {
            index = map.GetTileIndex(x, y + 1);
            map.TileVariations[index] = GetVariation(x, y + 1, map);
        }
    }
}