using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Topography/MapSettings", fileName = "MapSettings")]
public class MapSettings : ScriptableObject
{
    public enum MapDebugMode
    {
        CellMap = 0,
        IndexCellMap = 1,
        ContourMap = 2,
        ColorMap = 3,
        None = 4
    }

    [SerializeField]
    private MapDebugMode debugMode = MapDebugMode.None;

    [Space]

    [SerializeField]
    private int cellCount = 10;
    [SerializeField][Range(0f, 1f)]
    private float edgeThreshold;

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

    [Space]

    [SerializeField]
    private MapLayer[] mapLayers;

    public MapDebugMode DebugMode { get { return debugMode; } }

    public int CellCount { get { return cellCount; } }
    public float EdgeThreshold { get { return edgeThreshold; } }

    public float ContourThreshold { get { return contourThreshold; } }
    public int ContourWidth { get { return contourWidth; } }
    public Color ContourColor { get { return contourColor; } }

    public int IndexContour { get { return indexContour; } }
    public float IndexStrength { get { return indexStrength; } }

    public MapLayer[] MapLayers { get { return mapLayers; } }

    private void OnValidate()
    {
        cellCount = Mathf.Max(cellCount, 1);
        contourWidth = Mathf.Max(contourWidth, 0);
        indexContour = Mathf.Max(indexContour, 0);
        if(mapLayers.Length > 20){
            System.Array.Resize(ref mapLayers, 20);
        }
    }
}