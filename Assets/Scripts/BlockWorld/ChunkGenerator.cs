using UnityEngine;

[CreateAssetMenu(fileName = "Chunk Generation Settings", menuName = "Scriptable Objects/Chunk Generation Settings", order = 1)]
public class ChunkGenerationSettings : ScriptableObject
{
    [SerializeField] private int m_waterLevel = 64;
    [SerializeField] private int m_maxHeight = 128;

    public int MaxHeight => m_maxHeight;
    public int WaterLevel => m_waterLevel;
}

public class ChunkGenerator
{
    private readonly FastNoise m_noise;
    private readonly ChunkGenerationSettings m_settings;
    private readonly Blocks m_blocks;

    public ChunkGenerator(FastNoise noise, ChunkGenerationSettings settings, Blocks blocks)
    {
        m_noise = noise;
        m_settings = settings;
        m_blocks = blocks;
    }

    public Block[][,] GenerateChunk(Vector2 chunkIndex, int size)
    {
        Block[][,] blocks = new Block[m_settings.MaxHeight][,];
        
        for (int y = m_settings.MaxHeight - 1; y >= 0; y--)
        {
            blocks[y] = new Block[size + 2, size + 2];

            for (int z = 0; z < size + 2; z++)
            {
                for (int x = 0; x < size + 2; x++)
                {
                    float simX = chunkIndex.x * 16 + x - 1;
                    float simZ = chunkIndex.y * 16 + z - 1;

                    Block above = y == m_settings.MaxHeight - 1 ? m_blocks["Air"] : blocks[y + 1][x, z];

                    blocks[y][x, z] = GenerateBlock(chunkIndex, above, simX, y, simZ) ?? m_blocks["Air"];
                }
            }
        }

        return blocks;
    }

    private Block GenerateBlock(Vector2 chunkIndex, Block above, float x, float y, float z)
    {
        float simplex1 = m_noise.GetSimplex(x * 0.8f, z * 0.8f) * 10.0f;
        float simplex2 = m_noise.GetSimplex(x * 0.3f, z * 0.3f) * 10.0f *
                         (m_noise.GetSimplex(x * 0.3f, z * 0.3f) + 0.5f);

        float caveNoise = m_noise.GetSimplexFractal(x * 5.0f, y * 10.0f, z * 5.0f);
        float caveMask = m_noise.GetSimplex(x * 0.3f, z * 0.3f) + 0.3f;

        Block block = m_blocks["Air"];

        // Block generation is here
        if (y <= simplex1 + simplex2 + m_settings.WaterLevel)
        {
            if (above == null || above == m_blocks["Air"])
            {
                block = m_blocks["Grass"];
            }
            else
            {
                block = m_blocks["Dirt"];
            }
        }

        if (chunkIndex == Vector2.zero && x == 8 && z == 8 && y <= simplex1 + simplex2 + m_settings.WaterLevel && above == m_blocks["Air"])
        {
            block = m_blocks["Glass"];
        }

        if (chunkIndex == Vector2.zero && x == 8 && z == 8 && y <= simplex1 + simplex2 + m_settings.WaterLevel && above == m_blocks["Glass"])
        {
            block = m_blocks["Glowstone"];
        }

        if (simplex1 > 0.14f && y <= simplex1 + simplex2 + m_settings.WaterLevel && above == m_blocks["Air"])
        {
            block = m_blocks["SakuraLeaves"];
        }

        //if (caveNoise > Mathf.Max(caveMask, 0.2f))
        //{
        //    block = m_blocks["Air"];
        //}

        return block;
    }
}
