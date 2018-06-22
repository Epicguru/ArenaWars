using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MapIO
{
    private static byte[] tinyByteArray = new byte[2];
    private static byte[] byteArray;
    private static ushort[] ushortArray;

    public static void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string path = Path.Combine(GameIO.DataDirectory, "Test.txt");
            GameIO.EnsureDirectory(GameIO.DirectoryFromFile(path));

            ushort[] dataA = new ushort[Region.CHUNK_SIZE * Region.CHUNK_SIZE];
            ushort[] dataB = new ushort[Region.CHUNK_SIZE * Region.CHUNK_SIZE];
            for (int i = 0; i < dataA.Length; i++)
            {
                dataA[i] = (ushort)i;
                dataB[i] = (ushort)(dataA.Length - i - 1);
            }

            var fs = StartWrite(path);
            WriteValues(fs, dataA);
            WriteValues(fs, dataB);

            var data = ReadRegion(fs, 1);

            for (int i = 0; i < data.Length; i++)
            {
                Debug.Log(data[i]);
            }

            End(fs);
        }
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

    public static void WriteValues(FileStream fs, ushort[] values)
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

        fs.Write(bytes, 0, bytes.Length);
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

    public static ushort[] ReadRegion(FileStream fs, int regionIndex)
    {
        if(byteArray == null)
        {
            byteArray = new byte[Region.CHUNK_SIZE * Region.CHUNK_SIZE * 2];
        }
        if(ushortArray == null)
        {
            ushortArray = new ushort[Region.CHUNK_SIZE * Region.CHUNK_SIZE];
        }

        int size = byteArray.Length;
        int seekPos = size * regionIndex;
        fs.Seek(seekPos, SeekOrigin.Begin);
        int read = fs.Read(byteArray, 0, size);
        if(read != size)
        {
            Debug.LogError("The amount of bytes read ({0}) was not the expected {1}!".Form(read, size));
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
}