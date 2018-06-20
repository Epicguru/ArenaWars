using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    private static Dictionary<string, KeyCode[]> bindings;

    public static Vector2 ScreenMousePos { get; private set; }
    public static Vector2 MousePos { get; private set; }

    public static bool MouseOverAnyUI
    {
        get;
        private set;
    }

    public static void UpdateMouseUI(bool inUI)
    {
        MouseOverAnyUI = inUI;
    }

    public static void Init()
    {
        if(bindings != null)
        {
            return;
        }

        // Load default and current key bindings and merge...
        LoadAndMerge();
    }

    public static void Update()
    {
        ScreenMousePos = Input.mousePosition;
        Camera c = Camera.main;
        if(c == null)
        {
            Debug.LogWarning("Main camera is null, and it is used in the input manager to find world mouse pos.");
            return;
        }

        MousePos = c.ScreenToWorldPoint(ScreenMousePos);
    }

    private static void LoadAndMerge()
    {
        // Load default keys from resources...
        var defaultKeys = GameIO.ResourceToObject<Dictionary<string, KeyCode[]>>(GameIO.DefaultInputPath);
        int defNum = defaultKeys.Count;

        // Load current keys from file...
        var currentKeys = GameIO.FileToObject<Dictionary<string, KeyCode[]>>(GameIO.CurrentInputPath);
        if(currentKeys == null)
        {
            currentKeys = new Dictionary<string, KeyCode[]>();
        }
        // Merge current keys into default keys, but current keys have priority.
        foreach (var pair in currentKeys)
        {
            if (defaultKeys.ContainsKey(pair.Key))
            {
                defaultKeys[pair.Key] = pair.Value;
            }
            else
            {
                defaultKeys.Add(pair.Key, pair.Value);
            }
        }

        bindings = defaultKeys;
        Debug.Log(string.Format("Loaded {0} default key bindings, {1} current key bindings, and merged to create {2} bindings.", defNum, currentKeys.Count, defaultKeys.Count));
    }

    public static bool IsPressed(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        Init();

        if (!bindings.ContainsKey(input))
        {
            Debug.LogWarning("No key binding found for '{0}'! Will never return true.".Form(input));
            return false;
        }
        else
        {
            var keys = bindings[input];
            foreach (var key in keys)
            {
                if (Input.GetKey(key))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool IsDown(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        Init();

        if (!bindings.ContainsKey(input))
        {
            Debug.LogWarning("No key binding found for '{0}'! Will never return true.".Form(input));
            return false;
        }
        else
        {
            var keys = bindings[input];
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool IsUp(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        Init();

        if (!bindings.ContainsKey(input))
        {
            Debug.LogWarning("No key binding found for '{0}'! Will never return true.".Form(input));
            return false;
        }
        else
        {
            var keys = bindings[input];
            foreach (var key in keys)
            {
                if (Input.GetKeyUp(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}