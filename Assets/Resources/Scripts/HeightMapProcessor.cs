using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using EasyButtons;

public class HeightMapProcessor : MonoBehaviour
{
    private static Vector3 lum = new Vector3(0.2126f, 0.7152f, 0.0722f);

    private float Luminance(Color color){
        return lum.x * color.r + lum.y * color.g + lum.z * color.b;
    }

    private float remap01(float a, float b, float t){
        return (t - a) / (b - a);
    }

    [Button]
    private void ProcessMap(Texture2D map)
    {
        Texture2D normalizedMap = NormalizeMap(map);
    
        GameObject PlanePrim = GameObject.CreatePrimitive(PrimitiveType.Plane);
        MeshRenderer MeshRenderer = PlanePrim.GetComponent<MeshRenderer>();
    
        MeshRenderer.material = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));
        MeshRenderer.sharedMaterial.mainTexture = normalizedMap;

        //then Save To Disk as PNG
        byte[] bytes = normalizedMap.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if (!Directory.Exists(dirPath)){
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
    }

    private Vector2 GetTextureMinMax(Texture2D texture)
    {
        float min = Mathf.Infinity;
        float max = Mathf.NegativeInfinity;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixel = texture.GetPixel(x, y);
                float luminance = Luminance(pixel);
                if(luminance < min){
                    min = luminance;
                }
                if(luminance > max){
                    max = luminance;
                }
            }
        }
        return new Vector2(min, max);
    }

    private Texture2D NormalizeMap(Texture2D map)
    {
        Vector2 minMax = GetTextureMinMax(map);
        Color[,] heights = new Color[map.width, map.height];
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                Color pixel = map.GetPixel(x, y);
                float luminance = Luminance(pixel);
                float normalizedLum = remap01(minMax.x, minMax.y, luminance) * 255f;
                heights[x, y] = new Color(normalizedLum, normalizedLum, normalizedLum);
            }
        }
    
        return TextureFromHeightMap(heights);
    }
    
    public static Texture2D TextureFromHeightMap(Color[,] heightMap)
    {
        int mapWidth = heightMap.GetLength(0);
        int mapHeight = heightMap.GetLength(1);
        Color[] ColourMap = new Color[mapWidth * mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++){
                ColourMap[y * mapWidth + x] = heightMap[x, y];
            }
        }
        return TextureFromColourMap(ColourMap, mapWidth, mapHeight);
    }
    
    public static Texture2D TextureFromColourMap(Color[] colorMap, int mapWidth, int mapHeight)
    {
        Texture2D Texture = new Texture2D(mapWidth, mapHeight);
        Texture.filterMode = FilterMode.Point;
        Texture.wrapMode = TextureWrapMode.Clamp;
        Texture.SetPixels(colorMap);
        Texture.Apply();
        return Texture;
    }
}