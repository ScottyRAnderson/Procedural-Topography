static const float maxFloat = 3.402823466e+38;
static const float PI = 3.14159265359;

float remap01(float a, float b, float t) {
	return (t - a) / (b - a);
}

float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
	return minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld);
}