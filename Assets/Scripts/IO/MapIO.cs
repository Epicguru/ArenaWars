﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public static class MapIO
{
    private static byte[] tinyByteArray = new byte[2];
    private static byte[] byteArray;
    private static ushort[] ushortArray;

    public static void Zip(string sourceDirectory, string destinationPath)
    {
        if(sourceDirectory == null)
        {
            Debug.LogError("Null source directory string, cannot zip!");
        }

        if (!Directory.Exists(sourceDirectory))
        {
            Debug.LogError("Directory does not exist, cannot zip!");
        }

        if (destinationPath == null)
        {
            Debug.LogError("Null destination path string, cannot zip!");
        }

        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        GameIO.EnsureDirectory(GameIO.DirectoryFromFile(destinationPath));
        ZipFile.CreateFromDirectory(sourceDirectory, destinationPath, CompressionLevel.Optimal, false);
        Debug.Log("Zipped '{0}' to '{1}'.".Form(sourceDirectory, destinationPath));
    }

    public static void UnZip(string zipPath, string destinationDir, bool deleteExistingDir = true)
    {
        if(string.IsNullOrWhiteSpace(zipPath))
        {
            Debug.LogError("Null or blank zip file path! Cannot unzip!");
            return;
        }

        if (string.IsNullOrWhiteSpace(destinationDir))
        {
            Debug.LogError("Null or blank destination directory path! Cannot unzip!");
            return;
        }

        if (!File.Exists(zipPath))
        {
            Debug.LogError("Did not find zip file at '{0}', cannot unzip!".Form(zipPath));
            return;
        }

        if (Directory.Exists(destinationDir))
        {
            // Delete it?
            if (deleteExistingDir)
            {
                Directory.Delete(destinationDir, true);
            }
        }

        GameIO.EnsureDirectory(destinationDir);
        ZipFile.ExtractToDirectory(zipPath, destinationDir);
    }

    public static FileStream StartWrite(string path)
    {
        if(path == null || string.IsNullOrWhiteSpace(path))
        {
            Debug.LogError("Cannot start write, null or empty path supplied!");
            return null;
        }

        try
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            return fs;
        }
        catch (Exception e)
        {
            Debug.LogError("Cannot open file stream, so file cannot be written to! Write can not start!");
            Debug.LogError(e);
            return null;
        }
    }

    public static void End(FileStream fs)
    {
        if(fs != null)
        {
            fs.Dispose();
            fs.Close();
        }
        else
        {
            Debug.LogError("Null file stream, cannot close!");
        }
    }

    public static void WriteTileIDs(FileStream fs, ushort[] values)
    {
        if(fs == null)
        {
            Debug.LogError("Null file stream!");
            return;
        }
        if (!fs.CanWrite)
        {
            Debug.LogError("File stream cannot write!");
            return;
        }
        if(values == null || values.Length == 0)
        {
            Debug.LogError("Null or empty ushort array! Will not write!");
        }

        // Convert the ushort array to bytes...
        byte[] bytes = new byte[values.Length * 2];

        // Copy (and also convert) the ushort array into the byte array.
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);

        WriteBytes(fs, bytes);
    }

    public static void WriteTileVariations(FileStream fs, byte[] values)
    {
        WriteBytes(fs, values);
    }

    private static void WriteBytes(FileStream fs, byte[] values)
    {
        if (fs == null)
        {
            Debug.LogError("Null file stream!");
            return;
        }
        if (!fs.CanWrite)
        {
            Debug.LogError("File stream cannot write!");
            return;
        }
        if (values == null || values.Length == 0)
        {
            Debug.LogError("Null or empty ushort array! Will not write!");
        }

        fs.Write(values, 0, values.Length);
    }

    public static FileStream StartRead(string path)
    {
        if (path == null || string.IsNullOrWhiteSpace(path))
        {
            Debug.LogError("Cannot start read, null or empty path supplied!");
            return null;
        }

        try
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            return fs;
        }
        catch (Exception e)
        {
            Debug.LogError("Cannot open file stream, so file cannot be read from! Read can not start!");
            Debug.LogError(e);
            return null;
        }
    }

    public static ushort[] ReadRegionIDs(FileStream fs, int regionIndex)
    {
        if(byteArray == null)
        {
            byteArray = new byte[Region.SIZE * Region.SIZE * 2];
        }
        if(ushortArray == null)
        {
            ushortArray = new ushort[Region.SIZE * Region.SIZE];
        }

        int size = byteArray.Length;
        int seekPos = size * regionIndex;
        fs.Seek(seekPos, SeekOrigin.Begin);
        int read = fs.Read(byteArray, 0, size);
        if(read != size)
        {
            Debug.LogError("The amount of bytes read ({0}) was not the expected {1}!".Form(read, size));
            return null;
        }

        for (int i = 0; i < size / 2; i++)
        {
            // The writer saves the bytes in the wrong order, something to do with the copying of a ushort array to a byte array messes the order up.
            // Just flip the first and second byte to avoid wierd stuff from happening when loading.
            tinyByteArray[0] = byteArray[i * 2];
            tinyByteArray[1] = byteArray[i * 2 + 1];

            ushort value = BitConverter.ToUInt16(tinyByteArray, 0);
            ushortArray[i] = value;
        }

        return ushortArray;
    }

    public static ushort[] ReadAllTileIDs(FileStream fs, int totalRegions)
    {
        int tileCount = totalRegions * Region.SIZE * Region.SIZE;
        var ba = new byte[tileCount * 2];

        int size = ba.Length;
        int read = fs.Read(ba, 0, size);
        if (read != size)
        {
            Debug.LogError("The amount of bytes read ({0}) was not the expected {1}!".Form(read, size));
            return null;
        }

        var usa = new ushort[tileCount];

        for (int i = 0; i < size / 2; i++)
        {
            // The writer saves the bytes in the wrong order, something to do with the copying of a ushort array to a byte array messes the order up.
            // Just flip the first and second byte to avoid wierd stuff from happening when loading.
            tinyByteArray[0] = ba[i * 2];
            tinyByteArray[1] = ba[i * 2 + 1];

            ushort value = BitConverter.ToUInt16(tinyByteArray, 0);
            usa[i] = value;
        }

        ba = null;

        return usa;
    }

    public static byte[] ReadAllTileVariations(FileStream fs, int totalRegions)
    {
        int totalTiles = Region.SQR_SIZE * totalRegions;

        if(fs == null)
        {
            Debug.LogError("File stream is null, cannot read variations.");
            return null;
        }

        var ba = new byte[totalTiles];
        int read = fs.Read(ba, 0, ba.Length);

        if(read != ba.Length)
        {
            Debug.LogError("Read {0} bytes instead of the expected {1} bytes when reading all tile variations.".Form(read, ba.Length));
            return ba;
        }

        return ba;
    }
}