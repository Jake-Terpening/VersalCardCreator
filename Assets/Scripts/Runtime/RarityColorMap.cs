using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct RarityColorEntry
{
    [Tooltip("Rarity number, e.g., 1, 2, 3")]
    public int rarityID;

    public RarityColor colors;  // <-- use your existing struct here
}

[CreateAssetMenu(fileName = "RarityColorMap", menuName = "Cards/Rarity Color Map")]
public class RarityColorMap : ScriptableObject
{
    [Tooltip("List of rarity entries editable in inspector")]
    public List<RarityColorEntry> entries = new List<RarityColorEntry>();

    private Dictionary<int, RarityColor> _lookup;

    // Initialize lookup dictionary at runtime
    public void Initialize()
    {
        _lookup = new Dictionary<int, RarityColor>();
        foreach (var entry in entries)
        {
            if (!_lookup.ContainsKey(entry.rarityID))
                _lookup.Add(entry.rarityID, entry.colors);
        }
    }

    // Runtime getter
    public bool TryGetColors(int rarityID, out RarityColor colors)
    {
        if (_lookup == null) Initialize();
        return _lookup.TryGetValue(rarityID, out colors);
    }
}
