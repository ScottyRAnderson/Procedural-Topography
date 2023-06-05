using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode][RequireComponent(typeof(Camera))]
public class TopographicRenderer : MonoBehaviour
{
    public enum ComputeResolution
    {
        Res_256 = 256,
        Res_1080 = 1080,
        Res_1920 = 1920,
        Res_2560 = 2560,
        Res_3840 = 3840
    }

    [SerializeField]
    private MapSettings mapSettings;
    [SerializeField]
    private Texture2D heightMap;
    [SerializeField]
    private ComputeShader gaussianCompute;
    [SerializeField]
    private ComputeResolution computeResolution = ComputeResolution.Res_1080;
    [SerializeField]
    private int kernelSize = 27;

    private Material contourMat;
    private Material topographicMat;
    private RenderTexture blurTex;

    private void Awake() {
        UpdateBlurTexture();
    }

    private void OnValidate() {
        kernelSize = Mathf.Max(0, kernelSize);
        UpdateBlurTexture();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mapSettings == null) {
            return;
        }

        if (mapSettings.DebugMode == MapSettings.MapDebugMode.BlurMap) {
            if (blurTex == null) {
                UpdateBlurTexture();
            }
            Graphics.Blit(blurTex, destination);
            return;
        }

        BuildMaterials();

        contourMat.SetTexture("heightMap", blurTex);
        RenderTexture contourTex = TemporaryRenderTexture(source);
        Graphics.Blit(source, contourTex, contourMat);
        
        topographicMat.SetTexture("cellData", contourTex);
        topographicMat.SetTexture("heightMapBlur", blurTex);
        RenderTexture.ReleaseTemporary(contourTex);
        Graphics.Blit(source, destination, topographicMat);
    }

    public void UpdateBlurTexture()
    {
        int resolution = (int)computeResolution;
        blurTex = new RenderTexture(resolution, resolution, 24);
        blurTex.enableRandomWrite = true;
        blurTex.Create();

        Vector4 heightMap_TexelSize = new Vector4(1.0f / heightMap.width, 1.0f / heightMap.height, heightMap.width, heightMap.height);
        gaussianCompute.SetTexture(0, "result", blurTex);
        gaussianCompute.SetTexture(0, "heightMap", heightMap);
        gaussianCompute.SetVector("heightMap_TexelSize", heightMap_TexelSize);
        gaussianCompute.SetFloat("resolution", blurTex.width);
        gaussianCompute.SetInt("kernelSize", kernelSize / 2);
        gaussianCompute.SetFloat("sigma", mapSettings.EdgeSmoothness);
        gaussianCompute.Dispatch(0, blurTex.width / 8, blurTex.height / 8, 1);
    }

    private void BuildMaterials()
    {
        if(contourMat == null) {
            contourMat = new Material(Shader.Find("Custom/HeightCell"));
        }
        contourMat.SetInteger("cellCount", mapSettings.CellCount);
        contourMat.SetInteger("indexContour", mapSettings.IndexContour);

        if(topographicMat == null) {
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

    public static RenderTexture TemporaryRenderTexture(RenderTexture texture) {
        return RenderTexture.GetTemporary(texture.descriptor);
    }
}