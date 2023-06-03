using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode][RequireComponent(typeof(Camera))]
public class TopographicRenderer : MonoBehaviour
{
    [SerializeField]
    private ComputeShader gaussianCompute;
    [SerializeField]
    private MapSettings mapSettings;
    [SerializeField]
    private Texture2D heightMap;

    private RenderTexture blurTex;
    private Material contourMat;
    private Material topographicMat;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(blurTex != null) {
            Graphics.Blit(blurTex, destination);
        }
        else {
            Graphics.Blit(source, destination);
        }

        //BuildMaterials();

        //contourMat.SetTexture("heightMap", blurTex);
        //RenderTexture contourTex = TemporaryRenderTexture(source);
        //Graphics.Blit(source, contourTex, contourMat);
        //
        //topographicMat.SetTexture("cellData", contourTex);
        //RenderTexture.ReleaseTemporary(contourTex);
        //Graphics.Blit(source, destination, topographicMat);
    }

    public void UpdateBlurTexture()
    {
        blurTex = new RenderTexture(1080, 1080, 24);
        blurTex.enableRandomWrite = true;
        blurTex.Create();

        Vector4 heightMap_TexelSize = new Vector4(1.0f / heightMap.width, 1.0f / heightMap.height, heightMap.width, heightMap.height);

        gaussianCompute.SetTexture(0, "result", blurTex);
        gaussianCompute.SetTexture(0, "heightMap", heightMap);
        gaussianCompute.SetVector("heightMap_TexelSize", heightMap_TexelSize);
        gaussianCompute.SetFloat("resolution", blurTex.width);
        gaussianCompute.Dispatch(0, blurTex.width / 8, blurTex.height / 8, 1);
    }

    private void BuildMaterials()
    {
        if(contourMat == null){
            contourMat = new Material(Shader.Find("Custom/HeightCell"));
        }
        contourMat.SetInteger("cellCount", mapSettings.CellCount);
        contourMat.SetInteger("indexContour", mapSettings.IndexContour);

        if(topographicMat == null){
            topographicMat = new Material(Shader.Find("Custom/TopographicMap"));
        }
        topographicMat.SetInteger("debugMode", (int)mapSettings.DebugMode);
        topographicMat.SetFloat("edgeThreshold", mapSettings.EdgeThreshold);

        topographicMat.SetFloat("contourThreshold", mapSettings.ContourThreshold);
        topographicMat.SetInteger("contourWidth", mapSettings.ContourWidth);
        topographicMat.SetColor("contourColor", mapSettings.ContourColor);

        topographicMat.SetFloat("indexStrength", mapSettings.IndexStrength);
        topographicMat.SetTexture("heightMap", heightMap);

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