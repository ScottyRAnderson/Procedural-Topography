using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TopographicRenderer))][CanEditMultipleObjects]
public class TopographicRenderer_Inspector : Editor
{
    private static bool displayAdvanced = false;

    private TopographicRenderer rendererBase;

    private SerializedProperty mapSettings;
    private SerializedProperty heightMap;
    private SerializedProperty gaussianCompute;
    private SerializedProperty computeResolution;
    private SerializedProperty kernelSize;

    private void OnEnable()
    {
        rendererBase = target as TopographicRenderer;

        mapSettings = serializedObject.FindProperty("mapSettings");
        heightMap = serializedObject.FindProperty("heightMap");
        gaussianCompute = serializedObject.FindProperty("gaussianCompute");
        computeResolution = serializedObject.FindProperty("computeResolution");
        kernelSize = serializedObject.FindProperty("kernelSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(mapSettings);
        EditorGUILayout.PropertyField(heightMap);

        GUILayout.Space(5f);
        EditorGUI.indentLevel++;
        displayAdvanced = EditorHelper.Foldout(displayAdvanced, "Advanced Settings");
        if (displayAdvanced)
        {
            //EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(gaussianCompute);
            EditorGUILayout.PropertyField(computeResolution);
            EditorGUILayout.PropertyField(kernelSize);

            if (GUILayout.Button("Force Compute Update")) {
                rendererBase.UpdateBlurTexture();
            }
        }
        EditorGUI.indentLevel--;
        serializedObject.ApplyModifiedProperties();
    }
}