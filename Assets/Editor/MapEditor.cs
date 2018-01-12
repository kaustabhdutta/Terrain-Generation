/*Reference: GitHub
 * Kaustabh Dutta*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (inspectedType: typeof (MapGenerator))]
public class MapEditer : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (mapGen.autoUpdate)
        {
            mapGen.DrawMapInEditor();
        }

        DrawDefaultInspector();

        if(GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
