Shader "Custom/HeightCellShader"
{
    Properties
    {
        _SeaLevel ("Sea Level", range(0, 1)) = 0
        _NumCells ("Num Cells", Integer) = 1
        _EdgeThreshold ("Edge Threshold", range(0, 1)) = 0
        _HeightMap ("Height Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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

            float _SeaLevel;
            int _NumCells;
            float _EdgeThreshold;

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
                float4 finalCol;

                // Cell shaded value
                float height01 = tex2D(_HeightMap, i.uv);
                finalCol = height01;

                float cellSize = 1.0 / _NumCells;
                float height = 0;

                for (int c = 0; c < _NumCells; c++)
                {
                    if (height > height01) {
                        break;
                    }

                    height += cellSize;
                    finalCol = height;
                }

                // Edge detection
                float2 uv = i.uv;

                float x = _HeightMap_TexelSize.x;
                float y = _HeightMap_TexelSize.y;

                float s00 = tex2D(_HeightMap, i.uv + float2(-x, y));
                float s10 = tex2D(_HeightMap, i.uv + float2(-x, 0));
                float s20 = tex2D(_HeightMap, i.uv + float2(-x, -y));
                float s01 = tex2D(_HeightMap, i.uv + float2(0, y));
                float s21 = tex2D(_HeightMap, i.uv + float2(0, -y));
                float s02 = tex2D(_HeightMap, i.uv + float2(x, y));
                float s12 = tex2D(_HeightMap, i.uv + float2(x, 0));
                float s22 = tex2D(_HeightMap, i.uv + float2(x, -y));

                float sx = s00 + 2 * s10 + s20 - (s02 + 2 * s12 + s22);
                float sy = s00 + 2 * s01 + s02 - (s20 + 2 * s21 + s22);

                float g = sx * sx + sy * sy;

                if (g > _EdgeThreshold) {
                    return 1;
                }

                return finalCol;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
