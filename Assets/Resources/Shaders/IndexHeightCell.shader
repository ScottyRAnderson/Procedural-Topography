Shader "Custom/IndexHeightCell"
{
    Properties
    {
        _NumCells ("Num Cells", Integer) = 1
        _HeightMap ("Height Map", 2D) = "white" {}
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
            int _NumCells;
            int indexContour;

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
                    if (c == 0 || (c + 1) % indexContour == 0)
                    {
                        if (height > height01) {
                            break;
                        }

                        finalCol = height;
                        height += cellSize;
                        continue;
                    }

                    height += cellSize;
                }
                return finalCol;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}