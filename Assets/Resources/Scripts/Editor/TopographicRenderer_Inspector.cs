using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TopographicRenderer))][CanEditMultipleObjects]
public class TopographicRenderer_Inspector : Editor
{
    private static bool displayAdvancedData = false;
    private static bool displayLabelData = false;

    private TopographicRenderer rendererBase;

    private SerializedProperty mapSettings;
    private SerializedProperty heightMap;
    private SerializedProperty gaussianCompute;
    private SerializedProperty computeResolution;
    private SerializedProperty kernelSize;
    private SerializedProperty labelCanvas;
    private SerializedProperty labelText;

    private void OnEnable()
    {
        rendererBase = target as TopographicRenderer;

        mapSettings = serializedObject.FindProperty("mapSettings");
        heightMap = serializedObject.FindProperty("heightMap");
        gaussianCompute = serializedObject.FindProperty("gaussianCompute");
        computeResolution = serializedObject.FindProperty("computeResolution");
        kernelSize = serializedObject.FindProperty("kernelSize");
        labelCanvas = serializedObject.FindProperty("labelCanvas");
        labelText = serializedObject.FindProperty("labelText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawOverview();
        GUILayout.Space(5f);
        DrawLabelData();
        GUILayout.Space(5f);
        DrawAdvancedData();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawOverview()
    {
        using (new GUILayout.VerticalScope(EditorHelper.GetColoredStyle(EditorHelper.GroupBoxCol)))
        {
            EditorHelper.Header("Overview");
            using (new EditorGUI.DisabledScope(true)){
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
            }
            EditorGUILayout.PropertyField(mapSettings);
            EditorGUILayout.PropertyField(heightMap);
        }
    }

    private void DrawLabelData()
    {
        EditorGUI.indentLevel++;
        displayLabelData = EditorHelper.Foldout(displayLabelData, "Label Settings (Experimental)");
        if (displayLabelData)
        {
            EditorGUILayout.PropertyField(labelCanvas);
            EditorGUILayout.PropertyField(labelText);
            if (GUILayout.Button("Update Contour Text")) {
                rendererBase.SetShouldUpdateLabels();
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawAdvancedData()
    {
        EditorGUI.indentLevel++;
        displayAdvancedData = EditorHelper.Foldout(displayAdvancedData, "Advanced Settings");
        if (displayAdvancedData)
        {
            EditorGUILayout.PropertyField(gaussianCompute);
            EditorGUILayout.PropertyField(computeResolution);
            EditorGUILayout.PropertyField(kernelSize);
            if (GUILayout.Button("Force Compute Update")) {
                rendererBase.UpdateBlurTexture();
            }
        }
        EditorGUI.indentLevel--;
    }
}