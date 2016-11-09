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
    List<GameObject> layerObjects;

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
        while (selected_height_map != null) {
            layerObjects.Add(new GameObject("Layer_" + selected_layer.ToString()));
            MeshGenerator.GenerateMesh(selected_height_map, 1f, selected_depth, layerObjects[layerObjects.Count - 1], selectLayerColour(selected_layer));
            selected_depth += 0.2f;
            selected_height_map = current_height_map(borderedMap, selected_depth);
            selected_layer++;
        }        
    }
    
    Color32 selectLayerColour(int layer) {
        // Water
        if (layer < 8)
            return new Color32(0, 0, 40, 1);
        else if (layer < 11)
            return new Color32(0, 255, 255, 1);
        else if (layer < 12)
            return new Color32(0, 200, 170, 1);
        // Sand
        else if (layer < 14)
            return new Color32(255, 220, 128, 1);
        else if (layer < 16)
            return new Color32(210, 180, 0, 1);
        // Wheat
        else if (layer < 20)
            return new Color32(220, 240, 0, 1);
        else if (layer < 22)
            return new Color32(100, 255, 0, 1);
        // Forest
        else if (layer < 25)
            return new Color32(130, 190, 0, 1);
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
}