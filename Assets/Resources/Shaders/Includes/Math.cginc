static const float maxFloat = 3.402823466e+38;
static const float3 lum = float3(0.2126, 0.7152, 0.0722);

float remap01(float a, float b, float t) {
	return (t - a) / (b - a);
}

float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
	return minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld);
}

// Approximates the brightness of a RGB value. 
float luminance(float3 color) {
	return dot(lum, color);
}