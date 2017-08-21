using System.Collections;
using UnityEngine;
using UnityEditor;
using MapGen;

[CustomEditor(typeof(MapGeneration))]
public class MapGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGeneration mapGen = (MapGeneration)target;

        if (GUILayout.Button("Generate Map"))
            mapGen.Generate();
    }
}