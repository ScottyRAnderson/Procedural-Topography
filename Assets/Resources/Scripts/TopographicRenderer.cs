using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    [SerializeField]
    private Canvas labelCanvas;
    [SerializeField]
    private TextMeshProUGUI labelText;

    private Material contourMat;
    private Material topographicMat;
    private RenderTexture blurTex;

    private Texture2D contourData;
    private bool updateContourData;

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
        
        // Capture contour data for CPU work
        if(updateContourData) {
            updateContourData = false;
            contourData = new Texture2D(contourTex.width, contourTex.height, TextureFormat.RGB24, false);
            Rect regionToReadFrom = new Rect(0, 0, contourTex.width, contourTex.height);

            // Cannot simply use Graphics.copy here as this is not asynchronous, i.e. cannot copy data from GPU to CPU
            // .ReadPixels however allows GPU read-back, although this is a slow procedure so should be used only when necessary
            contourData.ReadPixels(regionToReadFrom, 0, 0, false);
            contourData.Apply();
            UpdateContourText();
        }
        
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

        Vector4 heightMap_TexelSize = new Vector4(1f / heightMap.width, 1f / heightMap.height, heightMap.width, heightMap.height);
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
        topographicMat.SetFloat("gradientShading", mapSettings.GradientShading);
        topographicMat.SetFloat("gradientAverage", mapSettings.GradientAverage);
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

    private static RenderTexture TemporaryRenderTexture(RenderTexture texture) {
        return RenderTexture.GetTemporary(texture.descriptor);
    }

    // Index contour text tag generation (Experimental)
    public void SetShouldUpdateLabels() {
        updateContourData = true;
    }

    public void UpdateContourText()
    {
        if(contourData == null) {
            return;
        }

        for (int i = labelCanvas.transform.childCount - 1; i >= 0; i--)
        {
            TextMeshProUGUI label = labelCanvas.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            if (!label){
                continue;
            }
            DestroyImmediate(label.gameObject);
        }

        List<contourPos> textPositions = IdentifyTextPositions(contourData);

        // We don't want all the positions here as there are too many...
        for (int i = 0; i < textPositions.Count; i++)
        {
            contourPos position = textPositions[i];
            position.FilterPoints(25);
            for (int j = 0; j < position.Points.Count; j++)
            {
                Vector2 point = position.Points[j];
                TextMeshProUGUI labelInstance = Instantiate(labelText);
                labelInstance.transform.SetParent(labelCanvas.transform);
                labelInstance.transform.position = new Vector3(point.x, point.y, 0f);
            }
        }
    }

    private List<contourPos> IdentifyTextPositions(Texture2D contourData)
    {
        List<contourPos> positions = new List<contourPos>();

        // Sobel edge detection
        for (int i = 0, j = contourData.width; i < j; i += 1) {
            for (int k = 0, l = contourData.height; k < l; k += 1) {
                Color cell00 = contourData.GetPixel(i + (-1), k + (1));
                Color cell01 = contourData.GetPixel(i + (-1), k + (0));
                Color cell02 = contourData.GetPixel(i + (-1), k + (-1));
                Color cell03 = contourData.GetPixel(i + (0), k + (1));
                Color cell04 = contourData.GetPixel(i + (0), k + (-1));
                Color cell05 = contourData.GetPixel(i + (1), k + (1));
                Color cell06 = contourData.GetPixel(i + (1), k + (0));
                Color cell07 = contourData.GetPixel(i + (1), k + (-1));

                // Evaluate green channel edges
                float s00 = cell00.g;
                float s10 = cell01.g;
                float s20 = cell02.g;
                float s01 = cell03.g;
                float s21 = cell04.g;
                float s02 = cell05.g;
                float s12 = cell06.g;
                float s22 = cell07.g;
                
                float sx = s00 + 2 * s10 + s20 - (s02 + 2 * s12 + s22);
                float sy = s00 + 2 * s01 + s02 - (s20 + 2 * s21 + s22);
                float g = sx * sx + sy * sy;
                if (g > 0.1f) {
                    float height = contourData.GetPixel(i, k).g;
                    Vector2 point = new Vector2(i, k);
                    contourPos pos = null;

                    for (int p = 0; p < positions.Count; p++)
                    {
                        if(positions[p].ContourHeight < height + 0.25f && positions[p].ContourHeight > height - 0.25f){ // positions[p].ContourHeight == height
                            pos = positions[p];
                            height = positions[p].ContourHeight;
                        }
                    }

                    if(pos == null) {
                        positions.Add(new contourPos(height, point));
                    }
                    else {
                        pos.AddPoint(point);
                    }
                }
            }
        }
        return positions;
    }

    private class contourPos
    {
        private float contourHeight;
        private List<Vector2> points;

        public float ContourHeight{ get { return contourHeight; } }
        public List<Vector2> Points { get { return points; } }

        public contourPos(float contourHeight, Vector2 initialPoint)
        {
            this.contourHeight = contourHeight;
            points = new List<Vector2>();
            AddPoint(initialPoint);
        }

        public void AddPoint(Vector2 point) {
            points.Add(point);
        }

        public void FilterPoints(float target)
        {
            List<Vector2> filteredPoints = new List<Vector2>();
            int increment = (int)(points.Count / target);
            for (int i = 0; i < points.Count; i += increment){
                filteredPoints.Add(points[i]);
            }
            points = new List<Vector2>(filteredPoints);

            for (int i = 0; i < points.Count; i++){
                for (int j = filteredPoints.Count - 1; j >= 0; j--){
                    if(points[i] != filteredPoints[j] && Vector2.Distance(points[i], filteredPoints[j]) < 200f) {
                        filteredPoints.RemoveAt(j);
                    }
                }
            }

            points = filteredPoints;
        }
    }
}