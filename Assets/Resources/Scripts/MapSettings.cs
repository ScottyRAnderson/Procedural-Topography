using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Topography/MapSettings", fileName = "MapSettings")]
public class MapSettings : ScriptableObject
{
    [SerializeField]
    private int cellCount = 10;
    [SerializeField][Range(0f, 1f)]
    private float contourThreshold;
    [SerializeField]
    private float contourSmooth = 30f;
    [SerializeField]
    private int contourWidth = 1;
    [SerializeField]
    private int indexContour = 5;
    [SerializeField]
    private Color contourColor = Color.black;

    [Space]

    [SerializeField]
    private MapLayer[] mapLayers;

    public int CellCount { get { return cellCount; } }
    public float ContourThreshold { get { return contourThreshold; } }
    public float ContourSmooth { get { return contourSmooth; } }
    public int ContourWidth { get { return contourWidth; } }
    public int IndexContour { get { return indexContour; } }
    public Color ContourColor { get { return contourColor; } }

    public MapLayer[] MapLayers { get { return mapLayers; } }

    private void OnValidate()
    {
        cellCount = Mathf.Max(cellCount, 1);
        contourThreshold = Mathf.Max(contourThreshold, 0f);
        contourSmooth = Mathf.Max(contourSmooth, 0f);
        contourWidth = Mathf.Max(contourWidth, 0);
        indexContour = Mathf.Max(indexContour, 0);
    }
}