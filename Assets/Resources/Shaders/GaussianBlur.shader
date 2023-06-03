Shader "Custom/GaussianBlur"
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
            #include "./Includes/Math.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D heightMap;
            float4 heightMap_TexelSize;

            float gauss(float x, float y, float sigma) {
                return  1.0f / (2.0f * PI * sigma * sigma) * exp(-(x * x + y * y) / (2.0f * sigma * sigma));
            }

            v2f vert(appdata_full v)
            {
                v2f o;
                o.uv = v.texcoord.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 o = 0;
                float sum = 0;
                float2 uvOffset;
                float weight;

                int KERNEL_SIZE = 15;
                float _Sigma = 100;

                for (float x = -KERNEL_SIZE / 2; x <= KERNEL_SIZE / 2; x++)
                {
                    for (float y = -KERNEL_SIZE / 2; y <= KERNEL_SIZE / 2; y++)
                    {
                        // Calculate the offset
                        uvOffset = i.uv;

                        uvOffset.x += heightMap_TexelSize.x * x;
                        uvOffset.y += heightMap_TexelSize.y * y;

                        // Determine the weights
                        weight = gauss(x, y, _Sigma);
                        o += tex2D(heightMap, uvOffset) * weight;
                        sum += weight;
                    }
                }

                o *= (1.0f / sum);
                return o;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}