using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {

    public int width;
    public int height;
    
    public bool autoUpdate;
    public int octaves;
    public float noiseScale;
    public int seed;
    public Vector2 offset;

    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    

    float[,] map;
    float[,] borderedMap;
    
    float[,] selected_height_map;
    List<GameObject> layerObjects;

    float[] square_distortion_columns;
    float[] square_distortion_rows; 

    void Start() {
        GenerateMap();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            foreach(GameObject terrain_layer in layerObjects) {
                Destroy(terrain_layer);
            }
            seed++;
            GenerateMap();
        }
    }

    public void GenerateMap() {
        float selected_depth = 0.2f;
        int borderSize = 1;
        int selected_layer = 0;

        layerObjects = new List<GameObject>();
        borderedMap = new float[width + borderSize * 2, height + borderSize * 2];
        map = new float[width, height];

        map = Noise.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistance, lacunarity, offset);
        
        for (int x = 0; x < borderedMap.GetLength(0); x++) {
            for (int y = 0; y < borderedMap.GetLength(1); y++) {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                } else {
                    borderedMap[x, y] = 0;
                }
            }
        }

        set_square_distortion();
        cliffMapGenerator();

        selected_height_map = current_height_map(borderedMap, selected_depth);        
        while (selected_height_map != null) {
            layerObjects.Add(new GameObject("Layer_" + selected_layer.ToString()));
            MeshGenerator.GenerateMesh(selected_height_map, 1f, square_distortion_rows, square_distortion_columns, selected_depth, layerObjects[layerObjects.Count - 1], selectLayerColour(selected_layer));
            selected_depth += 0.2f;
            selected_height_map = current_height_map(borderedMap, selected_depth);
            selected_layer++;
        }        
    }
    
    Color32 selectLayerColour(int layer) {
        // Water
        if (layer < 2)
            return new Color32(0, 0, 20, 1);
        else if (layer < 8)
            return new Color32(0, 0, 40, 1);
        else if (layer < 10)
            return new Color32(0, 160, 190, 1); 
        else if (layer < 11)
            return new Color32(0, 255, 255, 1); 
        else if (layer < 12)
            return new Color32(0, 255, 193, 1);
        // Sand
        else if (layer < 14)
            return new Color32(255, 220, 128, 1);
        else if (layer < 16)
            return new Color32(210, 180, 0, 1);
        // Wheat
        else if (layer < 20)
            return new Color32(220, 240, 0, 1);
        else if (layer < 22)
            return new Color32(200, 200, 0, 1);
        // Forest
        else if (layer < 25)
            return new Color32(0, 200, 0, 1);
        else if (layer < 28)
            return new Color32(50, 150, 0, 1);
        else if (layer < 32)
            return new Color32(0, 80, 0, 1);
        else if (layer < 35)
            return new Color32(0, 60, 0, 1);
        // Rock
        else if (layer < 38)
            return new Color32(80, 80, 0, 1);
        else if (layer < 40)
            return new Color32(80, 80, 80, 1);
        else if (layer < 43)
            return new Color32(165, 165, 200, 1);
        // Snow
        else
            return new Color32(255, 255, 255, 1);
    }

    float[,] current_height_map(float[,] global_map, float current_height) {
        bool isBlank = true;
        float[,] selected_height_map = new float[global_map.GetLength(0), global_map.GetLength(1)];
        for (int x = 0; x < global_map.GetLength(0); x++) {
            for (int y = 0; y < global_map.GetLength(1); y++) {
                if (global_map[x, y] >= current_height && global_map[x, y] <= current_height + 0.3f) { 
                    selected_height_map[x, y] = 1;
                    isBlank = false;
                }else if (global_map[x, y] >= current_height) {
                    selected_height_map[x, y] = 2;
                    isBlank = false;
                }
                else
                    selected_height_map[x, y] = 0;
            }
        }
        if (isBlank) return null;
        else return selected_height_map;
    }
    
    void set_square_distortion() {
        System.Random pseudorandom = new System.Random(seed);

        square_distortion_columns = new float[width + 2];
        square_distortion_rows = new float[height + 2];

        square_distortion_columns[0] = 0;
        square_distortion_rows[0] = 0;

        for (int x = 1; x < width + 2; x++) {
            square_distortion_columns[x] = x + ((float)pseudorandom.NextDouble() - 0.5f)/2;
        }
        for(int y = 1; y < height + 2; y++) {
            square_distortion_rows[y] = y + ((float)pseudorandom.NextDouble() - 0.5f) / 2;
        }
    }

    void cliffMapGenerator() {
        float[,] _cliff_map = new float[width + 2, height + 2];
        int randomFillPercent = 45;
        int iterations = 0;
        System.Random pseudoRandom = new System.Random(seed);

        for (int x = 0; x < width + 2; x++) {
            for (int y = 0; y < height + 2; y++) {
                _cliff_map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }

        while(iterations < 3) {
            for (int x = 0; x < width + 2; x++) {
                for (int y = 0; y < height + 2; y++) {
                    if (getSurroundingHeightCount(x, y, _cliff_map) > 4)
                        _cliff_map[x, y] = 1;
                    else if (getSurroundingHeightCount(x, y, _cliff_map) < 4)
                        _cliff_map[x, y] = 0;
                }
            }
            iterations++;
        }
        for (int x = 0; x < width + 2; x++) {
            for (int y = 0; y < height + 2; y++) {
                if(borderedMap[x, y] > 3.0f)
                    borderedMap[x,y] = borderedMap[x,y] + _cliff_map[x, y];
            }
        }
    }

    int getSurroundingHeightCount(int gridX, int gridY, float[,] _map) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if (neighbourX >= 0 && neighbourX < width + 2 && neighbourY >= 0 && neighbourY < height + 2) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += (_map[neighbourX, neighbourY] > 0)?1:0;
                    }
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
}