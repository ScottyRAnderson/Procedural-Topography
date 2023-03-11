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
    private Shader indexContourShader;
    [SerializeField]
    private Shader topograhicShader;

    private static Material defaultMat;

    private Material contourMat;
    private Material indexContourMat;
    private Material topographicMat;

    private void OnValidate()
    {
        
    }

    private void Init()
    {
        if (defaultMat == null){
            defaultMat = new Material(Shader.Find("Unlit/Texture"));
        }
        BuildMaterials();
    }

    private void BuildMaterials()
    {
        contourMat = new Material(contourShader);
        contourMat.SetTexture("_HeightMap", heightMap);
        contourMat.SetInteger("_NumCells", mapSettings.CellCount);

        indexContourMat = new Material(indexContourShader);
        indexContourMat.SetTexture("_HeightMap", heightMap);
        indexContourMat.SetInteger("_NumCells", mapSettings.CellCount);
        indexContourMat.SetInteger("indexContour", mapSettings.IndexContour);

        topographicMat = new Material(topograhicShader);
        topographicMat.SetFloat("_EdgeThreshold", mapSettings.ContourThreshold);
        topographicMat.SetInteger("_ContourWidth", mapSettings.ContourWidth);
        topographicMat.SetColor("_ContourColor", mapSettings.ContourColor);
        topographicMat.SetTexture("_HeightMap", heightMapFull);

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

    private void OnRenderImage(RenderTexture initialSource, RenderTexture finalDestination)
    {
        Init();
        RenderTexture contourTex = TemporaryRenderTexture(initialSource);
        Graphics.Blit(initialSource, contourTex, contourMat);
        
        RenderTexture indexContourTex = TemporaryRenderTexture(initialSource);
        Graphics.Blit(initialSource, indexContourTex, indexContourMat);

        topographicMat.SetTexture("_RenderTexture", contourTex);
        topographicMat.SetTexture("_RenderTextureIndex", indexContourTex);

        RenderTexture.ReleaseTemporary(contourTex);
        RenderTexture.ReleaseTemporary(indexContourTex);
        Graphics.Blit(initialSource, finalDestination, topographicMat);
    }

    public static RenderTexture TemporaryRenderTexture(RenderTexture texture){
        return RenderTexture.GetTemporary(texture.descriptor);
    }

    //[Button]
    //private void ProcessMap(Texture2D map)
    //{
    //    Texture2D heightCell = HeightCell(map);
    //
    //    GameObject PlanePrim = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //    MeshRenderer MeshRenderer = PlanePrim.GetComponent<MeshRenderer>();
    //
    //    MeshRenderer.material = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));
    //    MeshRenderer.sharedMaterial.mainTexture = heightCell;
    //}
    //
    //// Returns a cell shaded height map based on height
    //private Texture2D HeightCell(Texture2D map)
    //{
    //    Color[,] heights = new Color[map.width, map.height];
    //    for (int i = 0; i < map.width; i++)
    //    {
    //        for (int j = 0; j < map.height; j++)
    //        {
    //            Color pixel = map.GetPixel(i, j);
    //            heights[i, j] = pixel /2;
    //        }
    //    }
    //
    //    return TextureFromHeightMap(heights);
    //}
    //
    //public static Texture2D TextureFromHeightMap(Color[,] HeightMap)
    //{
    //    int MapScale = HeightMap.GetLength(0);
    //    Color[] ColourMap = new Color[MapScale * MapScale];
    //    for (int y = 0; y < MapScale; y++)
    //    {
    //        for (int x = 0; x < MapScale; x++){
    //            ColourMap[y * MapScale + x] = HeightMap[x, y];
    //        }
    //    }
    //    return TextureFromColourMap(ColourMap, MapScale);
    //}
    //
    //public static Texture2D TextureFromColourMap(Color[] ColourMap, int MapScale)
    //{
    //    Texture2D Texture = new Texture2D(MapScale, MapScale);
    //    Texture.filterMode = FilterMode.Point;
    //    Texture.wrapMode = TextureWrapMode.Clamp;
    //    Texture.SetPixels(ColourMap);
    //    Texture.Apply();
    //    return Texture;
    //}
}