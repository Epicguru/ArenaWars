using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputManagerGameObject))]
public class InputManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Save To File"))
        {
            (target as InputManagerGameObject).SaveToFile();
        }
    }
}