/*Reference: GitHub
 * Kaustabh Dutta*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour {

    public enum DrawMode {NoiseMap, ColorMap, Mesh, FalloffMap};
    public DrawMode drawMode;

    public TerrainNoise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 95;
    public bool useFlatShading;

    [Range(0,6)]
    public int editorPreviewLOD;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]                //A slider to ensure that the persistance value is always between 0 and 1
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useIslandGenerator;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    
    public bool autoUpdate;

    public TerrainType[] regions;
    static MapGenerator instance;

    float[,] islandGenMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        islandGenMap = IslandGenerator.GenerateIslandEffect(mapChunkSize);
    }

    public static int MapChunkSize
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapGenerator>();
            }
            if (instance.useFlatShading)
            {
                return 95;
            }
            else
            {
                return 239;
            }
        }
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        DisplayMap display = FindObjectOfType<DisplayMap>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD, useFlatShading), TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(IslandGenerator.GenerateIslandEffect(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod, useFlatShading);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i =0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    //Function to generate the noise map
    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = TerrainNoise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, centre+offset, normalizeMode);

        //Generating colored map
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];             //Array to save all colors used
        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                if (useIslandGenerator)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - islandGenMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)         //Loop to check which region the current height value falls into
                {
                    if(currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                       
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);

    }

    //To clamp the values to 0 or 1
    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        islandGenMap = IslandGenerator.GenerateIslandEffect(mapChunkSize);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


//Assigning colors to the different terrain types
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
