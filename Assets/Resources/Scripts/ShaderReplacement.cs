using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

[RequireComponent(typeof(Camera))]
public class ShaderReplacement : MonoBehaviour
{
    [SerializeField]
    private Shader shader;
    [SerializeField]
    private string renderType;

    private Camera attachedCamera;

    private void Awake(){
        attachedCamera = GetComponent<Camera>();
    }

    [Button]
    public void setReplacement()
    {
        if(attachedCamera == null) {
            attachedCamera = GetComponent<Camera>();
        }
        attachedCamera.SetReplacementShader(shader, renderType);
    }

    [Button]
    public void resetReplacement()
    {
        if (attachedCamera == null) {
            attachedCamera = GetComponent<Camera>();
        }
        attachedCamera.ResetReplacementShader();
    }
}