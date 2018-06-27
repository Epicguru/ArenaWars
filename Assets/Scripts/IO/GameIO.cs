using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public static class GameIO
{
    private static string PersistentDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
    private static JsonSerializerSettings jss;

    public static string DataDirectory
    {
        get
        {
            return Path.Combine(PersistentDataPath, "My Games", "Arena Wars");
        }
    }
    public static string StreamedDataDirectory
    {
        get
        {
            return Application.streamingAssetsPath;
        }
    }
    public static string ZippedMapsDirectory
    {
        get
        {
            return Path.Combine(StreamedDataDirectory, "Map Data");
        }
    }
    public static string UnzippedMapsDirectory
    {
        get
        {
            return Path.Combine(DataDirectory, "Map Data");
        }
    }
    public static string DefaultInputPath
    {
        get
        {
            return "Default Keys.txt";
        }
    }
    public static string CurrentInputPath
    {
        get
        {
            return Path.Combine(DataDirectory, "Input", "Current.keys");
        }
    }

    public static string FullResourcePath(string internalPath)
    {
        return Path.Combine(Application.dataPath, "Resources", internalPath);
    }

    public static string ObjectToJson(object o, Formatting formatting, bool unitySafe = true)
    {
        if (o == null)
            return null;

        if(jss == null)
            jss = new JsonSerializerSettings();

        jss.Formatting = formatting;
        if (unitySafe && jss.ContractResolver == null)
            jss.ContractResolver = new UnityContractResolver();
        else if (!unitySafe)
            jss.ContractResolver = null;

        // Make json from the object.
        string json = JsonConvert.SerializeObject(o, jss);

        return json;
    }

    public static void ObjectToFile(object o, string path, Formatting formatting = Formatting.Indented, bool unitySafe = true)
    {
        // Make the containing directory.
        EnsureDirectory(DirectoryFromFile(path));

        // Make json from the object.
        string json = ObjectToJson(o, formatting, unitySafe);

        // Write the json to file.
        File.WriteAllText(path, json);
    }

    public static void ObjectToResource(object o, string internalPath)
    {
        // Get the full path name. This is EDITOR ONLY!
        string fullPath = FullResourcePath(internalPath);

        // Make the containing directory.
        EnsureDirectory(DirectoryFromFile(fullPath));

        // Make json from the object.
        string json = JsonConvert.SerializeObject(o, Formatting.Indented);

        // Write the json to file.
        File.WriteAllText(fullPath, json);
    }

    public static T ResourceToObject<T>(string internalPath)
    {
        // Try to load from resources and deserialize.
        try
        {
            // Load json from the resources folder.
            if (internalPath.Contains("."))
            {
                internalPath = internalPath.Remove(internalPath.IndexOf("."));
            }
            string json = Resources.Load<TextAsset>(internalPath).text;
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }
        catch(Exception e)
        {
            Debug.LogError("Exception when loading or deserialzing json from resources '{0}'!".Form(internalPath));
            Debug.LogError(e);
            return default(T);
        }
    }

    public static T FileToObject<T>(string path)
    {
        // Check if it exists.
        if (!File.Exists(path))
        {
            Debug.LogError(string.Format("File not found: '{0}', cannot deserialize!", path));
            return default(T);
        }
        else
        {
            // Try to load from file and deserialize.
            try
            {
                string json = File.ReadAllText(path);
                T obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Exception when loading or deserialzing json from file '{0}'!", path));
                Debug.LogError(e);
                return default(T);
            }
        }
    }

    public static string DirectoryFromFile(string filePath)
    {
        if(filePath == null)
        {
            Debug.LogError("Null file path, cannot get directory path!");
        }
        string dir = Path.GetDirectoryName(filePath);
        return dir;
    }

    public static string FileNameFromPath(string filePath, bool extension)
    {
        if (filePath == null)
        {
            Debug.LogError("Null file path, cannot get name from path!");
        }
        return extension ? Path.GetFileName(filePath) : Path.GetFileNameWithoutExtension(filePath);
    }

    public static bool EnsureDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return false;
        }
        else
        {
            Directory.CreateDirectory(path);
            return true;
        }
    }
}