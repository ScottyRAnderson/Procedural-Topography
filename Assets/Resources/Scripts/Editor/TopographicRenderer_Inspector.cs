using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TopographicRenderer))][CanEditMultipleObjects]
public class TopographicRenderer_Inspector : Editor
{
    private SerializedProperty gaussianCompute;
    private SerializedProperty mapSettings;
    private SerializedProperty heightMap;

    private void OnEnable()
    {
        gaussianCompute = serializedObject.FindProperty("gaussianCompute");
        mapSettings = serializedObject.FindProperty("mapSettings");
        heightMap = serializedObject.FindProperty("heightMap");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if(GUILayout.Button("Dispatch Compute")) {
            (target as TopographicRenderer).UpdateBlurTexture();
        }
        EditorGUILayout.PropertyField(gaussianCompute);
        EditorGUILayout.PropertyField(mapSettings);
        EditorGUILayout.PropertyField(heightMap);
        serializedObject.ApplyModifiedProperties();
    }
}