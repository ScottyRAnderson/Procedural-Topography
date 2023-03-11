Shader "Custom/TopographicMap"
{
    Properties
    {
        _EdgeThreshold ("Edge Threshold", range(0, 1)) = 0
        _ContourWidth("Contour Width", Integer) = 1
        _ContourColor ("Contour Color", Color) = (1, 1, 1, 1)

        [Space]

        _HeightMap ("Height Map", 2D) = "white" {}
        _RenderTexture ("Render Texture", 2D) = "white" {}
        _RenderTextureIndex ("Render Texture Index", 2D) = "white" {}
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

            sampler2D _RenderTexture;
            float4 _RenderTexture_TexelSize;

            sampler2D _RenderTextureIndex;
            float4 _RenderTextureIndex_TexelSize;

            float _EdgeThreshold;
            float4 _ContourColor;
            int _ContourWidth;

            int numMapLayers;
            float mapThresholds[20];
            float4 mapLayers[20];

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
                // Edge detection
                float3 contourCol = 1;

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

                    float sx = s00 + 2 * s10 + s20 - (s02 + 2 * s12 + s22);
                    float sy = s00 + 2 * s01 + s02 - (s20 + 2 * s21 + s22);

                    float g = sx * sx + sy * sy;
                    if (g > _EdgeThreshold) {
                        contourCol = _ContourColor;
                    }

                    s00 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(-x, y)));
                    s10 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(-x, 0)));
                    s20 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(-x, -y)));
                    s01 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(0, y)));
                    s21 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(0, -y)));
                    s02 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(x, y)));
                    s12 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(x, 0)));
                    s22 = luminance(tex2D(_RenderTextureIndex, i.uv + float2(x, -y)));

                    sx = s00 + 2 * s10 + s20 - (s02 + 2 * s12 + s22);
                    sy = s00 + 2 * s01 + s02 - (s20 + 2 * s21 + s22);

                    g = sx * sx + sy * sy;
                    if (g > _EdgeThreshold) {
                        contourCol = float4(0, 1, 0, 1);
                    }

                    x += x;
                    y += y;
                }

                bool isIndexContour = colorDistance(contourCol, float3(0, 1, 0)) < .5;
                bool isContour = colorDistance(contourCol, 0) < .5 || isIndexContour;

                float contourData = isContour ? 1 : 0;
                float height01 = tex2D(_HeightMap, i.uv);

                float4 terrainCol = 0;
                for (int l = 0; l < numMapLayers; l++)
                {
                    float threshold = mapThresholds[l];
                    float4 col = toLinear(mapLayers[l]);
                    if (height01 <= threshold)
                    {
                        terrainCol = col;
                        break;
                    }
                }

                float contourStrength = contourData * (isIndexContour ? 1 : _ContourColor.a);
                return lerp(terrainCol, _ContourColor, contourStrength);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
