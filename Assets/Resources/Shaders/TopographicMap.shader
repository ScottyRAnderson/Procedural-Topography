Shader "Custom/TopographicMap"
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

            sampler2D heightMapBlur;
            float4 heightMapBlur_TexelSize;

            sampler2D cellData;
            float4 cellData_TexelSize;

            float edgeThreshold;
            float contourThreshold;
            float4 contourColor;
            int contourWidth;
            float indexStrength;

            int numMapLayers;
            float mapThresholds[20];
            float4 mapLayers[20];

            int debugMode;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.uv = v.texcoord.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float gradient(float2 uv)
            {
                float gradientAverage = 50;

                float height = tex2D(heightMapBlur, uv);
                float x = heightMapBlur_TexelSize.x;
                float y = heightMapBlur_TexelSize.y;

                // Compute the differentials by stepping over 1 in both directions.
                float dx = tex2D(heightMapBlur, uv + float2(x * gradientAverage, 0)) - height;
                float dy = tex2D(heightMapBlur, uv + float2(0, y * gradientAverage)) - height;
            
                // The "steepness" is the magnitude of the gradient vector
                // For a faster but not as accurate computation, you can just use abs(dx) + abs(dy)
                return sqrt(dx * dx + dy * dy);
            }

            // Identify contour lines through edge detection
            // Implements Sobel Opertor edge detection
            // Implementation Reference: https://homepages.inf.ed.ac.uk/rbf/HIPR2/sobel.htm
            float2 findContours(float2 uv)
            {
                float2 contourData = 1;
                float x = cellData_TexelSize.x;
                float y = cellData_TexelSize.y;
                for (int w = 0; w < contourWidth; w++)
                {
                    float4 cell00 = tex2D(cellData, uv + float2(-x, y));
                    float4 cell01 = tex2D(cellData, uv + float2(-x, 0));
                    float4 cell02 = tex2D(cellData, uv + float2(-x, -y));
                    float4 cell03 = tex2D(cellData, uv + float2(0, y));
                    float4 cell04 = tex2D(cellData, uv + float2(0, -y));
                    float4 cell05 = tex2D(cellData, uv + float2(x, y));
                    float4 cell06 = tex2D(cellData, uv + float2(x, 0));
                    float4 cell07 = tex2D(cellData, uv + float2(x, -y));
                    
                    // Evaluate red channel edges
                    float s00 = luminance(cell00.r);
                    float s10 = luminance(cell01.r);
                    float s20 = luminance(cell02.r);
                    float s01 = luminance(cell03.r);
                    float s21 = luminance(cell04.r);
                    float s02 = luminance(cell05.r);
                    float s12 = luminance(cell06.r);
                    float s22 = luminance(cell07.r);
                    
                    float sx = s00 + 2 * s10 + s20 - (s02 + 2 * s12 + s22);
                    float sy = s00 + 2 * s01 + s02 - (s20 + 2 * s21 + s22);
                    float g = sx * sx + sy * sy;
                    if (g > edgeThreshold) {
                        contourData.r = 0;
                    }
                    
                    // Evaluate green channel edges
                    s00 = luminance(cell00.g);
                    s10 = luminance(cell01.g);
                    s20 = luminance(cell02.g);
                    s01 = luminance(cell03.g);
                    s21 = luminance(cell04.g);
                    s02 = luminance(cell05.g);
                    s12 = luminance(cell06.g);
                    s22 = luminance(cell07.g);
                    
                    sx = s00 + 2 * s10 + s20 - (s02 + 2 * s12 + s22);
                    sy = s00 + 2 * s01 + s02 - (s20 + 2 * s21 + s22);
                    g = sx * sx + sy * sy;
                    if (g > edgeThreshold) {
                        contourData.g = 0;
                    }

                    // Increment sample width
                    x += x;
                    y += y;
                }
                return contourData;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Identify contour lines
                float2 contourData = findContours(i.uv);
                float gradientData = gradient(i.uv);

                // Unpack contour data
                bool isIndexContour = contourData.g < 1;
                bool isContour = contourData.r < 1 || isIndexContour;

                // Get map colour
                float4 terrainCol = 0;
                float mapHeight = tex2D(heightMap, i.uv);
                for (int l = 0; l < numMapLayers; l++)
                {
                    float threshold = mapThresholds[l];
                    float4 col = toLinear(mapLayers[l]);
                    if (mapHeight <= threshold)
                    {
                        terrainCol = col;
                        break;
                    }
                }

                // Handle debugging
                switch (debugMode)
                {
                    case 0:
                        return tex2D(heightMap, i.uv);
                        break;
                    case 2:
                        return tex2D(cellData, i.uv).r;
                        break;
                    case 3:
                        return tex2D(cellData, i.uv).g;
                        break;
                    case 4:
                        return luminance(float3(contourData, 0));
                        break;
                    case 5:
                        return terrainCol;
                        break;
                }

                gradientData = 1 - gradientData;
                gradientData = pow(gradientData, 2);
                if(mapHeight < contourThreshold) {
                    gradientData = 1;
                }
                terrainCol *= gradientData;

                // Attenuate contour lines based on if it's an index or not
                float contourStrength = ((isContour ? 1 : 0) * (isIndexContour ? indexStrength : contourColor.a)) * (mapHeight < contourThreshold ? 0 : 1);
                return lerp(terrainCol, toLinear(contourColor), contourStrength);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}