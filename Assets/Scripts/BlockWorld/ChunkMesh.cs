using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkMesh : MonoBehaviour, IPoolItem
{
    private enum Face
    {
        Front,
        Back,
        Left,
        Right,
        Top,
        Bottom
    }

    private struct FaceMap
    {
        public Vector3[] Map { get; private set; }

        public static FaceMap Make(short map)
        {
            FaceMap newMap = new FaceMap { Map = new Vector3[4] };

            map <<= 1;

            for (int i = 0; i < 4; i++)
            {
                float x = ((map >>= 1) & 0x1) == 0x1 ? 1.0f : 0.0f;
                float y = ((map >>= 1) & 0x1) == 0x1 ? 1.0f : 0.0f;
                float z = ((map >>= 1) & 0x1) == 0x1 ? 1.0f : 0.0f;

                newMap.Map[i] = new Vector3(x, y, z);
            }

            return newMap;
        }
    }

    private int m_size = 16;

    [SerializeField]
    [Button("Generate", "EditorGenerateMesh")]
    private Vector2 m_chunkIndex;

    private MeshFilter m_meshFilter;
    private MeshCollider m_meshCollider;
    private MeshRenderer m_meshRenderer;

    private volatile List<Vector3> m_verts = new List<Vector3>();
    private volatile List<int> m_indices = new List<int>();
    private volatile List<Vector2> m_uvs = new List<Vector2>();

    private static readonly Dictionary<Face, FaceMap> FaceBuilder = new Dictionary<Face, FaceMap>
    {
        { Face.Top, FaceMap.Make(0x7f2) },      // 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0
        { Face.Bottom, FaceMap.Make(0x948) },   // 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1
        { Face.Left, FaceMap.Make(0xb4) },      // 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0
        { Face.Right, FaceMap.Make(0xbd9) },    // 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1 
        { Face.Front, FaceMap.Make(0x2d0) },    // 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0 
        { Face.Back, FaceMap.Make(0x9bd) }      // 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1
    };

    public Vector2 ChunkIndex
    {
        get => m_chunkIndex;
        set => m_chunkIndex = value;
    }

    public FastNoise Noise { get; set; }

    private void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshCollider = GetComponent<MeshCollider>();
        m_meshRenderer = GetComponent<MeshRenderer>();
    }
    
    public void GenerateMesh(Block[][,] blocks)
    {
        m_verts.Clear();
        m_indices.Clear();
        m_uvs.Clear();

        // Populate faces.
        for (int y = 1; y < blocks.Length; y++)
        {
            for (int z = 1; z <= m_size; z++)
            {
                for (int x = 1; x <= m_size; x++)
                {
                    Block block = blocks[y][x, z];

                    if (!block.IsRenderable)
                    {
                        continue;
                    }

                    BuildBlock(x, y, z, block, blocks);
                }
            }
        }
    }

    public void SetMesh()
    {
        if (m_meshFilter == null)
        {
            m_meshFilter = GetComponent<MeshFilter>();
        }

        if (m_meshCollider == null)
        {
            m_meshCollider = GetComponent<MeshCollider>();
        }

        Mesh mesh = new Mesh
        {
            vertices = m_verts.ToArray(),
            triangles = m_indices.ToArray(),
            uv = m_uvs.ToArray()
        };

        mesh.RecalculateNormals();
        mesh.Optimize();

        m_meshFilter.mesh = mesh;
        m_meshFilter.sharedMesh = mesh;

        m_meshCollider.sharedMesh = mesh;

        m_meshRenderer.enabled = true;
    }

    private void BuildBlock(int x, int y, int z, Block block, IReadOnlyList<Block[,]> blocks)
    {
        Vector3 pos = new Vector3(x - 1, y, z - 1);

        // ToDo: Need to optimise this.
        Block left = blocks[y][x - 1, z];
        Block right = blocks[y][x + 1, z];
        Block front = blocks[y][x, z - 1];
        Block back = blocks[y][x, z + 1];
        Block top = y == blocks.Count - 1 ? null : blocks[y + 1][x, z];
        Block bottom = y == 0 ? null : blocks[y - 1][x, z];
        
        if (ShouldBuildFace(block, top)) BuildFace(pos, FaceBuilder[Face.Top], block.TileMap.Top);
        if (ShouldBuildFace(block, bottom)) BuildFace(pos, FaceBuilder[Face.Bottom], block.TileMap.Bottom);
        if (ShouldBuildFace(block, left)) BuildFace(pos, FaceBuilder[Face.Left], block.TileMap.Left);
        if (ShouldBuildFace(block, right)) BuildFace(pos, FaceBuilder[Face.Right], block.TileMap.Right);
        if (ShouldBuildFace(block, front)) BuildFace(pos, FaceBuilder[Face.Front], block.TileMap.Front);
        if (ShouldBuildFace(block, back)) BuildFace(pos, FaceBuilder[Face.Back], block.TileMap.Back);
    }

    private static bool ShouldBuildFace(Block current, Block next)
    {
        if (next == null)
        {
            return true;
        }

        if (!next.IsRenderable)
        {
            return true;
        }

        if (next.Transparent)
        {
            return current.IsRenderable;
        }

        return false;
    }

    private void BuildFace(Vector3 pos, FaceMap map, Tile tile)
    {
        int last = m_verts.Count;

        for (int component = 0; component < 4; component++)
        {
            m_verts.Add(pos + map.Map[component]);
        }

        m_indices.AddRange(new[] {
            last + 0, last + 1, last + 2,
            last + 2, last + 3, last + 0
        });

        m_uvs.AddRange(tile.Uvs);
    }

    public void ResetItem()
    {
        enabled = true;
    }

    public void Returned()
    {
        enabled = false;
        m_meshRenderer.enabled = false;
    }
}
