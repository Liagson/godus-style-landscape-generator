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
    float[,] selected_height_map;

    void Start() {
        GenerateMap();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            GenerateMap();
        }
    }

    public void GenerateMap() {
        float selected_depth = 0.2f;
        int borderSize = 1;
        float[,] borderedMap = new float[width + borderSize * 2, height + borderSize * 2];
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
        selected_height_map = current_height_map(borderedMap, selected_depth);

        int pos = 0;
        List<Color32> lista_color = new List<Color32>();
        lista_color.Add(new Color32(255, 0, 0, 1));
        lista_color.Add(new Color32(0, 255, 0, 1));
        lista_color.Add(new Color32(0, 0, 255, 1));
        while (selected_height_map != null) {
            MeshGenerator.GenerateMesh(selected_height_map, 1f, selected_depth, new GameObject("Layer_" + pos.ToString()), selectLayerColour(pos));
            selected_depth += 0.2f;
            selected_height_map = current_height_map(borderedMap, selected_depth);
            pos++;
        }        
    }
    
    Color32 selectLayerColour(int layer) {
        if (layer < 5)
            return new Color32(0, 0, 40, 1);
        else if (layer < 8)
            return new Color32(0, 255, 255, 1);
        else if (layer < 12)
            return new Color32(255, 220, 128, 1);
        else if (layer < 16)
            return new Color32(210, 180, 0, 1);
        else if (layer < 20)
            return new Color32(220, 240, 0, 1);
        else if (layer < 25)
            return new Color32(180, 150, 0, 1);
        else if (layer < 28)
            return new Color32(50, 150, 0, 1);
        else if (layer < 32)
            return new Color32(0, 80, 0, 1);
        else if (layer < 36)
            return new Color32(80, 80, 0, 1);
        else if (layer < 40)
            return new Color32(80, 80, 80, 1);
        else if (layer < 43)
            return new Color32(165, 165, 200, 1);
        else
            return new Color32(255, 255, 255, 1);
    }

    float[,] current_height_map(float[,] global_map, float current_height) {
        bool isBlank = true;
        float[,] selected_height_map = new float[global_map.GetLength(0), global_map.GetLength(1)];
        for (int x = 0; x < global_map.GetLength(0); x++) {
            for (int y = 0; y < global_map.GetLength(1); y++) {
                if (global_map[x, y] >= current_height) { 
                    selected_height_map[x, y] = 1;
                    isBlank = false;
                }
                else
                    selected_height_map[x, y] = 0;
            }
        }
        if (isBlank) return null;
        else return selected_height_map;
    }
    
    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += (int)map[neighbourX, neighbourY];
                    }
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }    
}