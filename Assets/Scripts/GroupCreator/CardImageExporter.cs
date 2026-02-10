using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardImageExporter : MonoBehaviour
{
    #region fields
    public enum CardType { Unit, Spell }

    CardType cardType = CardType.Unit; // Default to Unit

    // Unit fields
    string cardName = "New Card";
    string cardDescription = "Card Description";
    int cardAttack = 5;
    int cardDefense = 5; // Default health
    int cardLevel = 1; // Default level
    string cardTags = "Tags"; // Tags field for units

    // Spell fields
    string spellName = "New Spell";
    string spellDescription = "Spell Description";
    SpellType spellType; // Spell type enum
    string spellCondition = "Condition"; // Condition for spells
    Sprite spellImage;

    public enum SpellType { Counter, Continuous, Active } // Enum for spell types

    Sprite cardImage;

    float maxNameWidth = 230f; // Set an appropriate max width
    float maxTagWidth = 100f; // Set an appropriate max width
    string subFolder = "General";

    // Full path constants to match prefab hierarchy
    private const string unitCardNameString = "TextBack/CardName";
    private const string unitCardDescriptionString = "DescriptionBack/CardDescription";
    private const string unitCardAttackString = "AttackBack/CardAttack";
    private const string unitCardDefenseString = "DefenseBack/CardDefense";
    private const string unitCardLevelString = "LevelBack/CardLevel";
    private const string unitCardTagsString = "TagsBack/CardTags";
    private const string unitCardImageString = "ImageBack/CardImage";

    private const string spellCardNameString = "TextBack/CardName";
    private const string spellCardDescriptionString = "DescriptionBack/CardDescription";
    private const string spellCardTypeString = "SpellTypeBack/CardSpellType"; // Field for spell type
    private const string spellCardConditionString = "ConditionBack/CardCondition";
    private const string spellCardImageString = "ImageBack/CardImage";

    #endregion

#if UNITY_EDITOR
    public void CreateCard()
    {
        GameObject canvasObject = new GameObject("CardCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Load the appropriate prefab
        GameObject cardPrefab = (cardType == CardType.Unit)
            ? (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Cards/UnitBase.prefab", typeof(GameObject))
            : (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Cards/SpellBase.prefab", typeof(GameObject));

        GameObject newCard = Instantiate(cardPrefab, canvasObject.transform);

        // Assign CardData fields
        CardData cardData = new CardData();
        if (cardType == CardType.Unit)
        {
            cardData.name = cardName;
            cardData.type = "Unit";
            cardData.traits = cardTags;
            cardData.level = cardLevel.ToString();
            cardData.attack = cardAttack;
            cardData.defense = cardDefense;

            UpdateCardField(newCard, unitCardNameString, cardName);
            UpdateCardField(newCard, unitCardDescriptionString, cardDescription);
            UpdateCardField(newCard, unitCardAttackString, $"{cardAttack.ToString()} ATK");
            UpdateCardField(newCard, unitCardDefenseString, $"{cardDefense.ToString()} DEF");
            UpdateCardField(newCard, unitCardLevelString, cardLevel.ToString());
            UpdateCardField(newCard, unitCardTagsString, cardTags);
            UpdateCardImage(newCard, unitCardImageString, cardImage);
        }
        else if (cardType == CardType.Spell)
        {
            cardData.name = spellName;
            cardData.type = "Spell";
            string typeString = spellType.ToString();
            cardData.subtype = string.IsNullOrEmpty(typeString) ? "?" : typeString[0].ToString();
            cardData.condition = spellCondition;
            cardData.effect = spellDescription;

            UpdateCardField(newCard, spellCardNameString, spellName);
            UpdateCardField(newCard, spellCardDescriptionString, spellDescription);
            UpdateCardField(newCard, spellCardTypeString, spellType.ToString());
            UpdateCardImage(newCard, spellCardImageString, spellImage);

            // Handle condition visibility
            if (string.IsNullOrWhiteSpace(spellCondition))
            {
                newCard.transform.Find(spellCardConditionString).gameObject.SetActive(false);
            }
            else
            {
                UpdateCardField(newCard, spellCardConditionString, spellCondition);
            }
        }

        SaveCardPrefab(newCard, cardData);
    }

    // Save the card prefab
    void SaveCardPrefab(GameObject newCard, CardData cardData)
    {
        string folderPath = $"Assets/Cards/{subFolder}";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Cards", subFolder);
        }
        string fileName = $"{folderPath}/{cardData.name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(newCard, fileName);
        Debug.Log($"Card '{cardData.name}' saved as prefab at {fileName}");
    }
#endif

    // Helper for updating text fields (works in runtime too)
    void UpdateCardField(GameObject card, string path, string value)
    {
        var textComponent = card.transform.Find(path)?.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = value;
        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI not found at {path}");
        }
    }

    // Helper for updating images (works in runtime too)
    void UpdateCardImage(GameObject card, string path, Sprite image)
    {
        var imageComponent = card.transform.Find(path)?.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = image;
        }
        else
        {
            Debug.LogWarning($"Image component not found at {path}");
        }
    }
}
