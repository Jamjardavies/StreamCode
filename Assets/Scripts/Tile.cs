using UnityEngine;

public class Block
{
    // ToDo: Make this a scriptable object!
    public static Block Air = new Block { Name = "Air", IsRenderable = false };
    public static Block Grass = new Block { Name = "Grass", TileMap = new TileMap { Top = new Tile(1, 0), AllButTop = new Tile(0, 0) } };
    public static Block Glowstone = new Block { Name = "Glowstone", TileMap = new TileMap { All = new Tile(0, 1) } };

    public string Name { get; set; }
    public bool IsRenderable { get; set; } = true;
    public TileMap TileMap { get; set; }

}

public class TileMap
{
    public Tile Top { get; set; }
    public Tile Bottom { get; set; }
    public Tile Left { get; set; }
    public Tile Right { get; set; }
    public Tile Front { get; set; }
    public Tile Back { get; set; }

    public Tile All
    {
        set => Top = Bottom = Left = Right = Front = Back = value;
    }

    public Tile AllButTop
    {
        set => Bottom = Left = Right = Front = Back = value;
    }

    public Tile Sides
    {
        set => Left = Right = Front = Back = value;
    }
}


public class Tile
{
    private const float Padding = 0.001f;

    public Vector2[] Uvs { get; set; }

    public Tile(int x, int y)
        : this(new Vector2(x, y))
    {
    }

    public Tile(Vector2 tileIndex)
    {
        Uvs = new[]
        {
            new Vector2(tileIndex.x / 8.0f + Padding, tileIndex.y / 8.0f + Padding),
            new Vector2(tileIndex.x / 8.0f + Padding, (tileIndex.y + 1) / 8.0f - Padding),
            new Vector2((tileIndex.x + 1) / 8.0f - Padding, (tileIndex.y + 1) / 8.0f - Padding),
            new Vector2((tileIndex.x + 1) / 8.0f - Padding, tileIndex.y / 8.0f + Padding)
        };
    }
}