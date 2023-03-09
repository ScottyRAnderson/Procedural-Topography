Shader "Custom/ThickenLines"
{
    Properties
    {
        _ColorWater("Water Color", Color) = (1, 1, 1, 1)
        _SeaLevel("Sea Level", Range(0, 1)) = 0.0
        _ColorLow ("Low Color", Color) = (1, 1, 1, 1)
        _LowThreshold ("Low Threshold", Range(0, 1)) = 0.0
        _ColorMid ("Mid Color", Color) = (1, 1, 1, 1)
        _MidThreshold ("Mid Threshold", Range(0, 1)) = 0.5
        _ColorHigh ("High Color", Color) = (1, 1, 1, 1)
        _HighThreshold ("High Threshold", Range(0, 1)) = 0.8
        _ColorTop ("Top Color", Color) = (1, 1, 1, 1)

        [Space]

        _ContourWidth ("Contour Width", Integer) = 1
        _ContourColor ("Contour Color", Color) = (1, 1, 1, 1)

        [Space]

        _HeightMap ("Height Map", 2D) = "white" {}
        _RenderTexture ("Render Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./Includes/Math.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _HeightMap;
            float4 _HeightMap_ST;
            float4 _HeightMap_TexelSize;

            sampler2D _RenderTexture;
            float4 _RenderTexture_TexelSize;

            int _ContourWidth;
            float4 _ContourColor;

            float4 _ColorWater;
            float4 _ColorLow;
            float4 _ColorMid;
            float4 _ColorHigh;
            float4 _ColorTop;

            float _SeaLevel;
            float _LowThreshold;
            float _MidThreshold;
            float _HighThreshold;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 contours = tex2D(_RenderTexture, i.uv);
                bool isIndexContour = colorDistance(contours, float3(0, 1, 0)) < .5;
                bool isContour = colorDistance(contours, 0) < .5 || isIndexContour;

                float contourData = isContour ? 1 : 0;
                if (!isContour) {
                    contours = 1;
                }

                float height01 = tex2D(_HeightMap, i.uv);

                // Edge detection
                float2 uv = i.uv;

                float x = _RenderTexture_TexelSize.x;
                float y = _RenderTexture_TexelSize.y;

                int width = min(_ContourWidth, 100);
                for (int w = 0; w < width; w++)
                {
                    float s00 = luminance(tex2D(_RenderTexture, i.uv + float2(-x, y)));
                    float s10 = luminance(tex2D(_RenderTexture, i.uv + float2(-x, 0)));
                    float s20 = luminance(tex2D(_RenderTexture, i.uv + float2(-x, -y)));
                    float s01 = luminance(tex2D(_RenderTexture, i.uv + float2(0, y)));
                    float s21 = luminance(tex2D(_RenderTexture, i.uv + float2(0, -y)));
                    float s02 = luminance(tex2D(_RenderTexture, i.uv + float2(x, y)));
                    float s12 = luminance(tex2D(_RenderTexture, i.uv + float2(x, 0)));
                    float s22 = luminance(tex2D(_RenderTexture, i.uv + float2(x, -y)));

                    if (s00 <= 0.05f || s10 <= 0.05f || s20 <= 0.05f || s01 <= 0.05f || s21 <= 0.05f || s02 <= 0.05f || s12 <= 0.05f || s22 <= 0.05f){
                        contourData = 1;
                        break;
                    }

                    x += x;
                    y += y;
                }

                float4 terrainCol = 0;
                if (height01 > _HighThreshold) {
                    terrainCol = _ColorTop;
                }
                else if (height01 > _MidThreshold) {
                    terrainCol = _ColorHigh;
                }
                else if (height01 > _LowThreshold) {
                    terrainCol = _ColorMid;
                }
                else if (height01 > _SeaLevel) {
                    terrainCol = _ColorLow;
                }
                else {
                    return _ColorWater;
                }

                float contourStrength = contourData * (isIndexContour ? 1 : _ContourColor.a);
                return lerp(terrainCol, _ContourColor, contourStrength);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}