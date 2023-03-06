Shader "Custom/TopographicMap"
{
    Properties
    {
        _SeaLevel ("Sea Level", range(0, 1)) = 0
        _NumCells ("Num Cells", Integer) = 1
        _EdgeThreshold ("Edge Threshold", range(0, 1)) = 0

        [Space]

        _ContourWidth ("Contour Width", Float) = 1.0
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

            float _EdgeThreshold;
            float _ContourWidth;
            float4 _ContourColor;

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
                float4 finalCol = tex2D(_HeightMap, i.uv);

                // Edge detection
                float2 uv = i.uv;

                float x = _RenderTexture_TexelSize.x;
                float y = _RenderTexture_TexelSize.y;

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
                sx *= _ContourWidth;
                sy *= _ContourWidth;

                float g = sx * sx + sy * sy;

                if (g > _EdgeThreshold) {
                    return _ContourColor;
                }

                return finalCol;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
