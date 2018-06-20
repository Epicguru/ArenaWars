using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManagerGameObject : MonoBehaviour
{
    public EventSystem Events;

    [SerializeField]
    private List<NameKeyBinding> Bindings;

    public void SaveToFile()
    {
        Dictionary<string, KeyCode[]> dic = new Dictionary<string, KeyCode[]>();
        foreach (var item in Bindings)
        {
            dic.Add(item.Name, item.Keys);
        }
        GameIO.ObjectToResource(dic, GameIO.DefaultInputPath);
        Debug.Log(string.Format("Saved {0} inputs to '{1}'", Bindings.Count, GameIO.FullResourcePath(GameIO.DefaultInputPath)));
    }

    public void Awake()
    {
        Bindings = null;
    }

    public void Update()
    {
        if(Events != null)
        {
            InputManager.UpdateMouseUI(Events.IsPointerOverGameObject());
        }
        InputManager.Update();
    }
}

[System.Serializable]
public class NameKeyBinding
{
    public string Name;
    public KeyCode[] Keys;
}