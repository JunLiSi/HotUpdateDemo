using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DefaultAsset))]
public class LuaInspector : Editor
{
    public override void OnInspectorGUI()
    {
        string path = AssetDatabase.GetAssetPath(target);
        if (path.EndsWith(".lua"))
        {
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            string luaText = File.ReadAllText(path);
            GUILayout.TextArea(luaText);
        }
    }
}
