using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Topography/MapSettings", fileName = "MapSettings")]
public class MapSettings : ScriptableObject
{
    public enum MapDebugMode
    {
        HeightMap = 0,
        BlurMap = 1,
        CellMap = 2,
        IndexCellMap = 3,
        ContourMap = 4,
        SteepnessMap = 5,
        ColorMap = 6,
        None = 7
    }

    [SerializeField]
    private MapDebugMode debugMode = MapDebugMode.None;

    [Space]

    [SerializeField]
    private int cellCount = 10;
    [SerializeField][Range(0f, 1f)]
    private float edgeThreshold;
    [SerializeField]
    private float edgeSmoothness;

    [Space]

    [SerializeField][Range(0f, 1f)]
    private float contourThreshold;
    [SerializeField]
    private int contourWidth = 1;
    [SerializeField]
    private Color contourColor = Color.black;

    [Space]

    [SerializeField]
    private int indexContour = 5;
    [SerializeField][Range(0f, 1f)]
    private float indexStrength = 1f;

    [SerializeField]
    private float gradientShading = 2f;
    [SerializeField]
    private float gradientAverage = 50f;

    [Space]

    [SerializeField]
    private MapLayer[] mapLayers;

    public MapDebugMode DebugMode { get { return debugMode; } }

    public int CellCount { get { return cellCount; } }
    public float EdgeThreshold { get { return edgeThreshold; } }
    public float EdgeSmoothness { get { return edgeSmoothness; } }

    public float ContourThreshold { get { return contourThreshold; } }
    public int ContourWidth { get { return contourWidth; } }
    public Color ContourColor { get { return contourColor; } }

    public int IndexContour { get { return indexContour; } }
    public float IndexStrength { get { return indexStrength; } }

    public float GradientShading { get { return gradientShading; } }
    public float GradientAverage { get { return gradientAverage; } }

    public MapLayer[] MapLayers { get { return mapLayers; } }

    private void OnValidate()
    {
        cellCount = Mathf.Max(cellCount, 1);
        edgeSmoothness = Mathf.Max(edgeSmoothness, 0f);
        contourWidth = Mathf.Max(contourWidth, 0);
        indexContour = Mathf.Max(indexContour, 0);
        gradientShading = Mathf.Max(gradientShading, 0f);
        gradientAverage = Mathf.Max(gradientAverage, 0f);
        if(mapLayers.Length > 20){
            System.Array.Resize(ref mapLayers, 20);
        }
    }
}