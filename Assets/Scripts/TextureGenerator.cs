/*Reference: GitHub
 * Kaustabh Dutta*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
        	
	public static Texture2D TextureFromColourMap (Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;          //For making the color map crisp
        texture.wrapMode = TextureWrapMode.Clamp;          //For removing the other side of the map from the edges
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);


        Color[] colorMap = new Color[width * height];       //Generating 1D color map for 2D noise map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);       //Setting the colors for the pixels randomly
            }
        }

        return TextureFromColourMap(colorMap, width, height);
    }

}
