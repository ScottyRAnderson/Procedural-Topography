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

            // Vertical Sobel kernel
            static const float3x3 kernel_x = float3x3 (
                1, 0, -1,
                2, 0, -2,
                1, 0, -1
            );

            // Horizontal Sobel kernel
            static const float3x3 kernel_y = float3x3 (
                1, 2, 1,
                0, 0, 0,
                -1, -2, -1
            );

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

            float gradientShading;
            float gradientAverage;

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

            // Computes a simple 3x3 image convolution for a given kernel and pixel matrix
            float convolve(float3x3 pixels, float3x3 kernel)
            {
                float sum = 0;
                for (int x = 0; x < 3; x++)
                {  
                    for(int y = 0; y < 3; y++)
                    {
                        float kern = kernel[x][y];
                        float pixel = pixels[x][y];
                        sum += kern * pixel;
                    }
                }
                return sum;
            }

            // If returned value is high, an edge may exist for the given pixel matrix
            float sobel(float3x3 pixels)
            {
                float sx = convolve(pixels, kernel_x);
                float sy = convolve(pixels, kernel_y);
                return sqrt(pow(sx, 2) + pow(sy, 2));
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
                    // Retrieve colour data for 8 surrounding pixels
                    float4 cell00 = tex2D(cellData, uv + float2(-x, y));
                    float4 cell01 = tex2D(cellData, uv + float2(-x, 0));
                    float4 cell02 = tex2D(cellData, uv + float2(-x, -y));
                    float4 cell03 = tex2D(cellData, uv + float2(0, y));
                    float4 cell04 = tex2D(cellData, uv + float2(0, -y));
                    float4 cell05 = tex2D(cellData, uv + float2(x, y));
                    float4 cell06 = tex2D(cellData, uv + float2(x, 0));
                    float4 cell07 = tex2D(cellData, uv + float2(x, -y));

                    // Compute luminance values for pixels
                    // Normal contours held in red channel, index contours held in green channel
                    float2 s00 = float2(luminance(cell00.r), luminance(cell00.g));
                    float2 s10 = float2(luminance(cell01.r), luminance(cell01.g));
                    float2 s20 = float2(luminance(cell02.r), luminance(cell02.g));
                    float2 s01 = float2(luminance(cell03.r), luminance(cell03.g));
                    float2 s21 = float2(luminance(cell04.r), luminance(cell04.g));
                    float2 s02 = float2(luminance(cell05.r), luminance(cell05.g));
                    float2 s12 = float2(luminance(cell06.r), luminance(cell06.g));
                    float2 s22 = float2(luminance(cell07.r), luminance(cell07.g));

                    // Declare pixel matricies
                    float3x3 pixels_r = float3x3 (
                        s00.x, s01.x, s02.x,
                        s10.x, 0, s12.x,
                        s20.x, s21.x, s22.x
                    );
                    float3x3 pixels_g = float3x3 (
                        s00.y, s01.y, s02.y,
                        s10.y, 0, s12.y,
                        s20.y, s21.y, s22.y
                    );
                    
                    // Carry out Sobel Edge Detection
                    if (sobel(pixels_r) > edgeThreshold) {
                        contourData.r = 0;
                    }
                    if (sobel(pixels_g) > edgeThreshold) {
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

                // Isolate and shade heightmap steepness
                float gradientData = gradient(i.uv);
                gradientData = pow(1 - gradientData, gradientShading);
                if(mapHeight < contourThreshold) {
                    gradientData = 1;
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
                        return gradientData;
                        break;
                    case 6:
                        return terrainCol;
                        break;
                }

                // Contribute gradient shading
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