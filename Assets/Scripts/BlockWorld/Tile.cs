using System;
using UnityEngine;

[Serializable]
public class TileMap
{
    [SerializeField] private Tile m_top = null;
    [SerializeField] private Tile m_bottom = null;
    [SerializeField] private Tile m_left = null;
    [SerializeField] private Tile m_back = null;
    [SerializeField] private Tile m_front = null;
    [SerializeField] private Tile m_right = null;

    public Tile Top => m_top;
    public Tile Bottom => m_bottom;
    public Tile Left => m_left;
    public Tile Right => m_right;
    public Tile Front => m_front;
    public Tile Back => m_back;

    public void OnValidate()
    {
        Top.OnValidate();
        Bottom.OnValidate();
        Left.OnValidate();
        Right.OnValidate();
        Front.OnValidate();
        Back.OnValidate();
    }
}

[Serializable]
public class Tile
{
    private const float Padding = 0.001f;

    [SerializeField] private Vector2 m_index;
    [SerializeField] [HideInInspector] private Vector2[] m_uvs;

    public Vector2[] Uvs => m_uvs;

    public Tile(int x, int y)
        : this(new Vector2(x, y))
    {
    }

    public Tile(Vector2 tileIndex)
    {
        SetUVs(tileIndex);
    }

    public void OnValidate()
    {
        SetUVs(m_index);
    }

    private void SetUVs(Vector2 tileIndex)
    {
        m_uvs = new[]
        {
            new Vector2(tileIndex.x / 8.0f + Padding, tileIndex.y / 8.0f + Padding),
            new Vector2(tileIndex.x / 8.0f + Padding, (tileIndex.y + 1) / 8.0f - Padding),
            new Vector2((tileIndex.x + 1) / 8.0f - Padding, (tileIndex.y + 1) / 8.0f - Padding),
            new Vector2((tileIndex.x + 1) / 8.0f - Padding, tileIndex.y / 8.0f + Padding)
        };
    }
}