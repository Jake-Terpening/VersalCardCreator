using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AffinityData
{
    public string affinityName;     // Sun, Moon, Land, Sky
    public Color backdropColor;     // Color for the affinity background
    public Sprite symbolSprite;     // Icon shown on the card
}

[CreateAssetMenu(fileName = "AffinityMap", menuName = "Cards/Affinity Map")]
public class AffinityMap : ScriptableObject
{
    [SerializeField] private List<AffinityData> affinities = new List<AffinityData>();

    private Dictionary<string, AffinityData> affinityLookup;

    [SerializeField] private AffinityData defaultAffinity;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        affinityLookup = new Dictionary<string, AffinityData>();

        foreach (var affinity in affinities)
        {
            if (string.IsNullOrWhiteSpace(affinity.affinityName)) continue;

            string key = affinity.affinityName.ToLower();

            if (!affinityLookup.ContainsKey(key))
                affinityLookup.Add(key, affinity);
            else
                Debug.LogWarning($"Duplicate affinity name in AffinityMap: {affinity.affinityName}");
        }
    }

    public AffinityData GetAffinity(string affinityName)
    {
        AffinityData data;
        string affinityNameLower = affinityName.ToLower();
        if (affinityLookup == null || affinityLookup.Count != affinities.Count)
        {
            BuildLookup();
        }
        if (string.IsNullOrWhiteSpace(affinityNameLower) || !affinityLookup.ContainsKey(affinityNameLower))
        {
            data = defaultAffinity;
        }
        else
        {
            data = affinityLookup[affinityNameLower];
        }
        return data;
    }

}
