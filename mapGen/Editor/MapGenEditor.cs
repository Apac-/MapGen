using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGen))]
public class MapGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGen mapGen = (MapGen)target;

        if (GUILayout.Button("Generate Map"))
            mapGen.Generate();
    }
}