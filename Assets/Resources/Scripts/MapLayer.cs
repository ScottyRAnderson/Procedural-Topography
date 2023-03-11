using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapLayer
{
    [SerializeField][Range(0f, 1f)]
    private float threshold;
    [SerializeField]
    private Color layerColor;

    public float Threshold { get { return threshold; } }
    public Color LayerColor { get { return layerColor; } }
}