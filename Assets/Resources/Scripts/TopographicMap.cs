using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

[ExecuteInEditMode]
public class TopographicMap : MonoBehaviour
{
    [SerializeField]
    private MapSettings mapSettings;
    [SerializeField]
    private Texture2D heightMap;
    [SerializeField]
    private Texture2D heightMapFull;

    [Space]

    [SerializeField]
    private Shader contourShader;
    [SerializeField]
    private Shader topograhicShader;

    private Material contourMat;
    private Material topographicMat;

    private void OnRenderImage(RenderTexture initialSource, RenderTexture finalDestination)
    {
        BuildMaterials();
        RenderTexture contourTex = TemporaryRenderTexture(initialSource);
        Graphics.Blit(initialSource, contourTex, contourMat);

        topographicMat.SetTexture("cellData", contourTex);
        RenderTexture.ReleaseTemporary(contourTex);
        Graphics.Blit(initialSource, finalDestination, topographicMat);
    }

    private void BuildMaterials()
    {
        contourMat = new Material(contourShader);
        contourMat.SetTexture("heightMap", heightMap);
        contourMat.SetInteger("cellCount", mapSettings.CellCount);
        contourMat.SetInteger("indexContour", mapSettings.IndexContour);

        topographicMat = new Material(topograhicShader);
        topographicMat.SetInteger("debugMode", (int)mapSettings.DebugMode);
        topographicMat.SetFloat("contourThreshold", mapSettings.ContourThreshold);
        topographicMat.SetInteger("contourWidth", mapSettings.ContourWidth);
        topographicMat.SetColor("contourColor", mapSettings.ContourColor);
        topographicMat.SetTexture("heightMap", heightMapFull);

        MapLayer[] mapLayers = mapSettings.MapLayers;
        float[] mapThresholds = new float[20];
        Color[] mapColors = new Color[20];
        for (int i = 0; i < mapLayers.Length; i++)
        {
            MapLayer layer = mapLayers[i];
            mapThresholds[i] = layer.Threshold;
            mapColors[i] = layer.LayerColor;
        }

        topographicMat.SetInt("numMapLayers", mapLayers.Length);
        topographicMat.SetFloatArray("mapThresholds", mapThresholds);
        topographicMat.SetColorArray("mapLayers", mapColors);
    }

    public static RenderTexture TemporaryRenderTexture(RenderTexture texture){
        return RenderTexture.GetTemporary(texture.descriptor);
    }
}