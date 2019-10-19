using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Noise;
using System.Threading.Tasks;
using System;

public class DensityGenerator : MonoBehaviour
{
    [Range(-1.0f, 1.0f)]
    public float Surface;
    public int ChunkSize;
    public Vector3Int ChunkPosition;
    private float[,,] density;
    private Vector3Int[,,] positions;
    private Mesh mesh;
    public int Seed = 1;
    public State initialized = State.NotInitialized;
    public struct Tile { public int x; public int y; }
    public enum Direction { north, east, south, west, up, down };
    private MeshCollider coll;
    private MeshFilter filter;
    private MeshData meshData;
    private bool isActive = false;
    public bool playerIsHere = false;
    public enum State {
        NotInitialized,
        Initializing,
        ReadyToLoad,
        Loading,
        Initialized,

    }

    const float tileSize = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // GenerateViaMesh(ChunkSize);
        // GenerateViaBox();
    }

    public virtual Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();
        tile.x = 0;
        tile.y = 0;

        return tile;
    }


    public virtual Vector2[] FaceUVs(Direction direction)
    {
        Vector2[] UVs = new Vector2[4];
        Tile tilePos = TexturePosition(direction);

        UVs[0] = new Vector2(tileSize * tilePos.x + tileSize,
            tileSize * tilePos.y);
        UVs[1] = new Vector2(tileSize * tilePos.x + tileSize,
            tileSize * tilePos.y + tileSize);
        UVs[2] = new Vector2(tileSize * tilePos.x,
            tileSize * tilePos.y + tileSize);
        UVs[3] = new Vector2(tileSize * tilePos.x,
            tileSize * tilePos.y);

        return UVs;
    }

    protected virtual MeshData FaceDataUp
        (int x, int y, int z, MeshData meshData)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.up));
        return meshData;
    }

    protected virtual MeshData FaceDataDown
        (int x, int y, int z, MeshData meshData)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.down));
        return meshData;
    }

    protected virtual MeshData FaceDataNorth
        (int x, int y, int z, MeshData meshData)
    {
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.north));
        return meshData;
    }

    protected virtual MeshData FaceDataEast
        (int x, int y, int z, MeshData meshData)
    {
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.east));
        return meshData;
    }

    protected virtual MeshData FaceDataSouth
        (int x, int y, int z, MeshData meshData)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.south));
        return meshData;
    }

    protected virtual MeshData FaceDataWest
        (int x, int y, int z, MeshData meshData)
    {
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.west));
        return meshData;
    }

    void GenerateViaMesh(int size, bool generate_density = true)
    {
        meshData = new MeshData();
        meshData.useRenderDataForCol = true;
        print("Generating mesh");
        if (generate_density) {
            Noise generator = new Noise(Seed);
            density = new float[size + 1, size + 1, size + 1];
            positions = new Vector3Int[size + 1, size + 1, size + 1];
            print("Generating density");
            for (int x = 0; x < size + 1; x++)
            {
                for (int y = 0; y < size + 1; y++)
                {
                    for (int z = 0; z < size + 1; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z) + ChunkPosition * size;
                        positions[x, y, z] = pos;
                        float top = (size - pos.y);
                        density[x, y, z] = generator.Evaluate(new Vector3(pos.x / ((float) (size * 4)), pos.y / ((float) (size * 4)), pos.z / ((float) (size * 4)))) + 2.0f * top * top * top * top / (size * size * size * size);
                    }
                }
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (isSquare(x, y, z))
                    {
                        var pos = positions[x, y, z];
                        if (!isSquare(x, y + 1, z)) {
                            meshData = FaceDataUp(pos.x, pos.y, pos.z, meshData);
                        }
                        if (!isSquare(x, y - 1, z)) {
                            meshData = FaceDataDown(pos.x, pos.y, pos.z, meshData);
                        }
                        if (!isSquare(x, y, z + 1)) {
                            meshData = FaceDataNorth(pos.x, pos.y, pos.z, meshData);
                        }
                        if (!isSquare(x, y, z - 1)) {
                            meshData = FaceDataSouth(pos.x, pos.y, pos.z, meshData);
                        }
                        if (!isSquare(x + 1, y, z)) {
                            meshData = FaceDataEast(pos.x, pos.y, pos.z, meshData);
                        }
                        if (!isSquare(x - 1, y, z)) {
                            meshData = FaceDataWest(pos.x, pos.y, pos.z, meshData);
                        }                        
                    }
                }
            }
        }
    
        initialized = State.ReadyToLoad;

        // mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;


        /*
        foreach (Vector3 square in squares) {
            GameObject obj = new GameObject();
            obj.transform.parent = this.transform;
            var collider = obj.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
            collider.transform.position = square;
        }
        */


    }

    bool isVisible(int x_center, int y_center, int z_center)
    {

        for (int x = x_center - 1; x <= x_center + 1; x++)
        {
            if (x == x_center)
            {
                continue;
            }
            if (!isSquare(x, y_center, z_center))
            {
                return true;
            }
        }
        for (int y = y_center - 1; y <= y_center + 1; y++)
        {
            if (y == y_center)
            {
                continue;
            }
            if (!isSquare(x_center, y, z_center))
            {
                return true;
            }
        }
        for (int z = z_center - 1; z <= z_center + 1; z++)
        {
            if (z == z_center)
            {
                continue;
            }
            if (!isSquare(x_center, y_center, z))
            {
                return true;
            }
        }
        return false;
    }

    bool isSquare(int x_start, int y_start, int z_start)
    {
        if (x_start < 0 || y_start < 0 || z_start < 0)
        {
            return false;
        }
        if (x_start >= ChunkSize || y_start >= ChunkSize || z_start >= ChunkSize)
        {
            return false;
        }
        return density[x_start, y_start, z_start] > Surface;
        /* 
        // print(string.Format("{0}, {1}, {2}", x_start, y_start, z_start));
        for (int x = x_start; x <= x_start + 1; x++)
        {
            for (int y = y_start; y <= y_start + 1; y++)
            {
                for (int z = z_start; z <= z_start + 1; z++)
                {
                    if (density[x, y, z] <= Surface)
                    {
                        squares[x_start, y_start, z_start] = Square.IsNotSquare;
                        return false;
                    }
                }
            }
        }
        squares[x_start, y_start, z_start] = Square.IsSquare;
        return true;
        */
    }
    // Update is called once per frame
    public void Load() {
        if (initialized == State.ReadyToLoad) 
        {
            
            initialized = State.Loading;
            filter.mesh.Clear();
            filter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            filter.mesh.vertices = meshData.vertices.ToArray();
            filter.mesh.triangles = meshData.triangles.ToArray();

            filter.mesh.uv = meshData.uv.ToArray();
            
            filter.mesh.RecalculateNormals();

            coll.sharedMesh = null;
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = meshData.colVertices.ToArray();
            mesh.triangles = meshData.colTriangles.ToArray();
            mesh.RecalculateNormals();
            
            // coll.sharedMesh = mesh;
            initialized = State.Initialized;
        }
    }

    public void Unload() {
        Destroy(mesh);
        Destroy(filter.mesh);
    }

    public void ForceReload() {
        initialized = State.Initializing;
        GenerateViaMesh(ChunkSize, false);
        Load();
        coll.sharedMesh = mesh;
        isActive = true; 
    }

    public void RemoveBlock(Vector3Int position) {
        var pos = position - ChunkPosition * ChunkSize;
        for (int x = 0; x < ChunkSize; x++) {
            for (int z = 0; z < ChunkSize; z++) {
                density[x, pos.y, z] = 0.0f;
            }
        }
        
        
    }

    void Update()
    {
        if (initialized == State.NotInitialized)
        {
            initialized = State.Initializing;
            filter = GetComponent<MeshFilter>();
            coll = GetComponent<MeshCollider>();            
            Task.Run(() => GenerateViaMesh(ChunkSize));
        }
        else if (initialized == State.Initialized) 
        {
            if (!isActive && playerIsHere) {
                coll.sharedMesh = mesh;
                isActive = true;
            }   
            if (isActive && !playerIsHere) {
                coll.sharedMesh = null;
                isActive = false;
            }
        }
    }
}
