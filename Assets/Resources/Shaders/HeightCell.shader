Shader "Custom/HeightCell"
{
    Properties { }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D heightMap;
            int cellCount;
            int indexContour;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.uv = v.texcoord.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Cell shade the original map
                float mapHeight = tex2D(heightMap, i.uv);
                float4 cellData = 0;

                float height = 0;
                float cellSize = 1.0 / cellCount;
                for (int c = 0; c < cellCount; c++)
                {
                    if (height > mapHeight) {
                        break;
                    }

                    // Pack index contours into their own colour channel
                    if (c == 0){
                        cellData.rg = height;
                    }
                    else if ((c + 1) % indexContour == 0){
                        cellData.g = height;
                    }
                    else{
                        cellData.r = height;
                    }

                    height += cellSize;
                }
                return cellData;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}