/*Reference: GitHub
 * Kaustabh Dutta*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainNoise {

    public enum NormalizeMode {Local, Gloabal};

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        //float[] noiseMap = new float[mapWidth * mapHeight];

        System.Random prng = new System.Random(seed);           //prng = pseudo random number generator for generating
                                                                //noise at random locations on the map
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i=0; i<octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;          //Storing the minimum and maximum
        float minLocalNoiseHeight = float.MaxValue;          //noise values for the terrain

        Perlin.Reseed();

        float halfWidth = mapWidth / 2f;        //In order to get the central map noise values
        float halfHeight = mapHeight / 2f;       //when zooming in on the terrain map

        for(int y=0; y<mapHeight; y++)
        {
            for(int x=0; x<mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for(int i=0; i<octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;      //Deciding the sample points 
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency ;     //where the noise will be sampled and the number of samples

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)* 2 - 1;         
                    noiseHeight += perlinValue * amplitude;

                    /*float perlinValue = Perlin.Noise(sampleX, sampleY);
                    minLocalNoiseHeight = Mathf.Min(minLocalNoiseHeight, noiseHeight);
                    maxLocalNoiseHeight = Mathf.Max(maxLocalNoiseHeight, noiseHeight);*/

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap [x, y] = noiseHeight;
            }
        }
        for(int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                { 
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
	
}
