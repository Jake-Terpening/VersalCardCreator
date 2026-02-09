using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public string name;
    public string type;  // "Unit" or "Spell"
    public string subtype;  // Used for spells, e.g., "Counter", "Active"
    public string condition;  // Used for spells (could be empty for some)
    public string effect;
    public string traits;  // Used for units (e.g., "Flying", "Trample")
    public string level;  // Used for units
    public int attack;  // Used for units
    public int defense;  // Used for units
}

public class CardManager : MonoBehaviour
{
    public TextAsset cardCSV;  // This is where the CSV will be stored

    // List to hold our parsed card data
    private List<CardData> cards = new List<CardData>();

    void Start()
    {
        ParseCSV();
    }

    private void ParseCSV()
    {
        string[] rows = cardCSV.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rows.Length; i++) // Skip the header row
        {
            string[] columns = rows[i].Split(',');

            string name = columns[0].Trim();
            if (string.IsNullOrEmpty(name)) continue; // Skip invalid rows

            string type = columns[8].Trim(); // Last column for Type

            if (type.Equals("Unit", StringComparison.OrdinalIgnoreCase))
            {
                // Parse Unit
                string level = columns[1].Trim();
                string traits = columns[2].Trim();
                string effect = columns[3].Trim();
                string attack = columns[4].Trim();
                string defense = columns[5].Trim();

                Debug.Log($"Unit - Name: {name}, Level: {level}, Traits: {traits}, Effect: {effect}, Attack: {attack}, Defense: {defense}");
            }
            else if (type.Equals("Spell", StringComparison.OrdinalIgnoreCase))
            {
                // Parse Spell
                string subtype = columns[6].Trim();
                string condition = columns[7].Trim();
                string effect = columns[3].Trim(); // Shared Effect column

                Debug.Log($"Spell - Name: {name}, Subtype: {subtype}, Condition: {condition}, Effect: {effect}");
            }
            else
            {
                Debug.LogWarning($"Unrecognized card type in row {i + 1}: {rows[i]}");
            }
        }
    }

}
