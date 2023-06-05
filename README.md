# Topographic Map
Procedural topographic map generator based on real-world heightmap data. Sample heightmap data provided including data sampled from Yosemite (California), the Moon, Mars.
<br>
Most data taken from [Tangrams Heightmapper](https://tangrams.github.io/heightmapper/)

## Features
* **Contour Identification**
<br> Procedurally identifies contour lines through a two pass edge detection algorithm utilising a gaussian model blur pass for line smoothing.
* **Flat Shading**
<br> Flat cell-based shading for underlying height map to better distinguish contour lines and better resemble a real-world topographic map. Colouration is based on height.
* **Exposed Parameters**
<br> All parameters contributing to the final image are serialized to allow fast modification of colours, heights, contour data, etc... Heightmaps may be provided to the system as .png or .jpg

## Development Video
Below is a development video detailing the creation process of this system,
<br>[Procedural Topography](https://www.youtube.com/watch?v=t89De819YMA)

![TopographicMapA](https://raw.githubusercontent.com/ScottyRAnderson/Images/master/topographic-map_feature_1.jpg)
![TopographicMapB](https://raw.githubusercontent.com/ScottyRAnderson/Images/master/topographic-map_feature_2.jpg)