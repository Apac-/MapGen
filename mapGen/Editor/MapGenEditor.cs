using System.Collections;
using UnityEngine;
using UnityEditor;
using MapGen;

[CustomEditor(typeof(MapGenerator))]
public class MapGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator mapGen = (MapGenerator)target;

        if (GUILayout.Button("Generate Map"))
            mapGen.Generate();
    }
}