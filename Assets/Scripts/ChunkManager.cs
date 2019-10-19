using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class ChunkManager : MonoBehaviour
{
    public GameObject player;

    public int ChunkSize;
    public int Seed;
    [Range(-1.0f, 1.0f)]
    public float Surface;
    private Vector3Int activeChunk;
    private Vector3Int nextChunk;
    public int ChunkRadius;
    public UnityEngine.Object prefab;
    private List<Vector3Int> chunksToInit = new List<Vector3Int>();
    private Dictionary<Vector3Int, DensityGenerator> chunkMap;
    // Start is called before the first frame update
    void Start()
    {   
        chunkMap = new Dictionary<Vector3Int, DensityGenerator>();       
        Refresh(true);
    }

    void UnloadChunks() {
        Vector3Int[] keys = new Vector3Int[chunkMap.Keys.Count];
        chunkMap.Keys.CopyTo(keys, 0);
        foreach (var key in keys) {
            var chunk = chunkMap[key];
            int x = chunk.ChunkPosition.x - nextChunk.x;
            int z = chunk.ChunkPosition.z - nextChunk.z;
        
            int rad2 = (ChunkRadius + 3) * (ChunkRadius * 3);
            if ( x * x >  rad2 || z * z > rad2) {
                chunk.Unload();
                Destroy(chunk.gameObject);
                chunkMap.Remove(key);
            }
        }
    }

    DensityGenerator CreateChunk(Vector3Int newPosition) {
        GameObject newObject = Instantiate(prefab) as GameObject;
        DensityGenerator chunk = newObject.GetComponent<DensityGenerator>();
        chunk.Seed = Seed;
        chunk.Surface = Surface;
        chunk.ChunkSize = ChunkSize;
        chunk.ChunkPosition = newPosition;
        return chunk;
    }

    void LoadChunks() {
        chunksToInit.Clear();
        for (int i = ChunkRadius * -1; i <= ChunkRadius; i++) {
            for (int j = ChunkRadius * -1; j <= ChunkRadius; j++) {
                Vector3Int pos = new Vector3Int(i + nextChunk.x, 0, j + nextChunk.z);
                bool isActiveChunk = i <= 1 && i >= -1 && j <= 1 && j >= -1;
                if (chunkMap.ContainsKey(pos)) {
                    chunkMap[pos].playerIsHere = isActiveChunk;
                    continue;
                }
                chunksToInit.Add(pos);
            }
        }
    }

    public void RemoveBlock(Vector3Int position) {
        chunkMap[activeChunk].RemoveBlock(position);
    }
    
    public void ForceReload() {
        chunkMap[activeChunk].ForceReload();
    }

    void Refresh(bool force) {
        int x = Mathf.RoundToInt(player.transform.position.x) / ChunkSize;
        int z = Mathf.RoundToInt(player.transform.position.z) / ChunkSize;
        nextChunk.x = x;
        nextChunk.z = z;
        if (activeChunk.x != x || activeChunk.z != z || force) {
            UnloadChunks();
            LoadChunks();
            activeChunk.x = x;
            activeChunk.z = z;
        }
        int max_init = 15;
        
        for (int i = chunksToInit.Count - 1; i > 0 && i >  chunksToInit.Count - 4; i--) {
            var pos = chunksToInit[i];
            chunksToInit.RemoveAt(i);
            bool isActiveChunk = pos.x == activeChunk.x && pos.y == activeChunk.y;
            var chunk = CreateChunk(pos);
            chunk.playerIsHere = isActiveChunk;
            chunkMap[pos] = chunk;
        }
        max_init = 10;
        
        foreach (var key in chunkMap.Keys) {
            var chunk = chunkMap[key];
            if (chunk.initialized == DensityGenerator.State.ReadyToLoad) {
                chunk.Load();
                
                if (--max_init <= 0) break;
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        Refresh(false);
    }
}
