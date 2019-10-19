using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int Width;
    public int Height;
    [Range(0, 100)]
    public int RandomFillPercent;

    public bool UseRandomSeed;
    public string Seed;

    int[,] map;

    void Start() {
        GenerateMap();
    }

    void Update() {
        if (Input.GetMouseButton(0)) {
            GenerateMap();
        }
    }

    void GenerateMap() {
        map = new int[Width, Height];
        RandomFillMap();
        for (int i = 0; i < 5; i++) {
            SmoothMap();
        }
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, 1);

    }

    void RandomFillMap() {
        if (UseRandomSeed) {
            Seed = Time.time.ToString();
        }
        System.Random rnd = new System.Random(Seed.GetHashCode());

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1) {
                    map[x, y] = 1;
                } else {
                    map[x, y] = (rnd.Next(0, 100) < RandomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                int neighbourTiles = GetSurroundingWallCount(x, y);
                if (neighbourTiles > 4) {
                    map[x, y] = 1;
                } else if (neighbourTiles < 4) {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0; 
        for (int neightbourX = gridX - 1; neightbourX <= gridX + 1; neightbourX++) {
            for (int neightbourY = gridY - 1; neightbourY <= gridY + 1; neightbourY++) {
                if (neightbourX >= 0 && neightbourX < Width && neightbourY >= 0 && neightbourY < Height) {
                    if (neightbourY != gridY || neightbourX != gridX) {
                        wallCount += map[neightbourX, neightbourY];
                    }                   
                } else {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    void OnDrawGizmos() {
        if (map != null) {
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-Width/2 + x + 0.5f, 0, -Height/2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
