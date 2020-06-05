using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Blocks")]
public class Blocks : ScriptableObject
{
    [SerializeField] private Block[] m_blocks = null;

    private readonly Dictionary<string, Block> m_blockLookup = new Dictionary<string, Block>();

    public Block[] AllBlocks => m_blocks;

    public Block this[string blockName] => m_blockLookup.ContainsKey(blockName) ? m_blockLookup[blockName] : null;

    private void OnEnable()
    {
        if (m_blocks == null)
        {
            return;
        }

        // Populate a block map.
        foreach (Block block in m_blocks)
        {
            m_blockLookup[block.Name] = block;
        }
    }

    private void OnValidate()
    {
        foreach (Block block in m_blocks)
        {
            block.OnValidate();
        }
    }
}

[Serializable]
public class Block
{
    [SerializeField] private string m_blockName = "";
    [SerializeField] private bool m_isRenderable = true;
    [SerializeField] private bool m_transparent = false;
    [SerializeField] private TileMap m_tileMap = null;

    public string Name => m_blockName;
    public bool IsRenderable => m_isRenderable;
    public bool Transparent => m_transparent;
    public TileMap TileMap => m_tileMap;
    
    public void OnValidate()
    {
        m_tileMap.OnValidate();
    }
}