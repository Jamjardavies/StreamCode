using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField]
    private ChunkGenerationSettings m_chunkGenSettings = null;

    [SerializeField]
    private Blocks m_blocks = null;

    [SerializeField]
    private ChunkMesh m_chunkPrefab = null;

    [SerializeField]
    private Transform m_children = null;

    [SerializeField]
    private Transform m_player = null;

    [SerializeField]
    private int m_seed = 1337;

    [SerializeField]
    private int m_chunkDistance = 2;

    private FastNoise m_noise;

    private Pool<ChunkMesh> m_chunkPool;
    private ChunkGenerator m_generator;

    private readonly List<Vector2> m_chunkRange = new List<Vector2>();
    private readonly List<Tuple<Task, ChunkMesh>> m_loadingChunks = new List<Tuple<Task, ChunkMesh>>();
    private readonly Dictionary<Vector2, ChunkMesh> m_loadedChunks = new Dictionary<Vector2, ChunkMesh>();
    private readonly List<Vector2> m_chunksToUnload = new List<Vector2>();
    
    private void Awake()
    {
        m_noise = new FastNoise(m_seed);

        m_chunkPool = new Pool<ChunkMesh>("Chunk Pool", m_chunkPrefab, 256);
        m_generator = new ChunkGenerator(m_noise, m_chunkGenSettings, m_blocks);

        // Generate the starting chunk.
        LoadChunk(Vector2.zero);
        
        StartCoroutine(Populate());
    }

    private void Update()
    {
        // Get player position.
        if (m_player == null)
        {
            return;
        }

        int chunkX = Mathf.FloorToInt(m_player.position.x / 16.0f);
        int chunkZ = Mathf.FloorToInt(m_player.position.z / 16.0f);

        m_chunkRange.Clear();

        int chunkDistance = m_chunkDistance / 2;

        // Generate a circle around the player to generate chunks.
        for (int z = -chunkDistance; z < chunkDistance; z++)
        {
            for (int x = -chunkDistance; x < chunkDistance; x++)
            {
                Vector2 chunk = new Vector2(chunkX + x, chunkZ + z);

                m_chunkRange.Add(chunk);

                if (m_loadedChunks.ContainsKey(chunk))
                {
                    continue;
                }

                LoadChunk(chunk);
            }
        }

        // Unload chunks
        foreach (Vector2 loadedChunk in m_loadedChunks.Keys.Where(loadedChunk => !m_chunkRange.Contains(loadedChunk)))
        {
            m_chunksToUnload.Add(loadedChunk);
        }

        foreach (Vector2 unload in m_chunksToUnload)
        {
            ChunkMesh chunk = m_loadedChunks[unload];
            m_loadedChunks.Remove(unload);

            m_chunkPool.Return(chunk);
        }

        m_chunksToUnload.Clear();
    }

    private void LoadChunk(Vector2 pos)
    {
        // Don't load an already loaded chunk.
        if (m_loadedChunks.ContainsKey(pos))
        {
            return;
        }
        
        ChunkMesh mesh = m_chunkPool.Get();

        if (mesh == null)
        {
            return;
        }

        m_loadedChunks.Add(pos, mesh);
        m_loadingChunks.Add(new Tuple<Task, ChunkMesh>(GenerateChunk(pos, mesh), mesh));
    }

    private IEnumerator Populate()
    {
        while (true)
        {
            if (m_loadingChunks.Count > 0)
            {
                IEnumerable<Tuple<Task, ChunkMesh>> completed = m_loadingChunks.Where(t => t.Item1.IsCompleted);

                foreach (Tuple<Task, ChunkMesh> item in completed)
                {
                    item.Item2.SetMesh();
                }

                m_loadingChunks.RemoveAll(t => t.Item1.IsCompleted);
            }
            
            yield return null;
        }
    }

    private async Task GenerateChunk(Vector2 pos, ChunkMesh mesh)
    {
        mesh.transform.parent = m_children;
        mesh.Noise = m_noise;
        mesh.transform.position = new Vector3(pos.x * 16, 0, pos.y * 16);
        mesh.ChunkIndex = pos;

        Block[][,] blocks = await Task.Run(() => m_generator.GenerateChunk(pos, 16));

        await Task.Run(() => mesh.GenerateMesh(blocks));
    }
}

