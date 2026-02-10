using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardCreatorWindow : EditorWindow
{
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

    [MenuItem("Tools/Card Creator")]
    public static void ShowWindow()
    {
        GetWindow<CardCreatorWindow>("Card Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create a New Card", EditorStyles.boldLabel);

        cardType = (CardType)EditorGUILayout.EnumPopup("Card Type", cardType);

        if (cardType == CardType.Unit)
        {
            // Unit fields
            cardName = EditorGUILayout.TextField("Card Name", cardName);
            cardDescription = EditorGUILayout.TextField("Description", cardDescription);
            cardAttack = EditorGUILayout.IntField("Power", cardAttack);
            cardDefense = EditorGUILayout.IntField("Health", cardDefense);
            cardLevel = EditorGUILayout.IntField("Level", cardLevel);
            cardTags = EditorGUILayout.TextField("Card Tags", cardTags);
            cardImage = (Sprite)EditorGUILayout.ObjectField("Card Image", cardImage, typeof(Sprite), allowSceneObjects: false);
            maxNameWidth = EditorGUILayout.FloatField("Max Name Width", maxNameWidth);
        }
        else if (cardType == CardType.Spell)
        {
            // Spell fields
            spellName = EditorGUILayout.TextField("Spell Name", spellName);
            spellDescription = EditorGUILayout.TextField("Description", spellDescription);
            spellType = (SpellType)EditorGUILayout.EnumPopup("Spell Type", spellType);
            spellCondition = EditorGUILayout.TextField("Condition", spellCondition);
            spellImage = (Sprite)EditorGUILayout.ObjectField("Spell Image", spellImage, typeof(Sprite), allowSceneObjects: false);
        }

        subFolder = EditorGUILayout.TextField("Deck", subFolder);

        if (GUILayout.Button("Create Card"))
        {
            CreateCard();
        }
    }

    void CreateCard()
    {
        GameObject canvasObject = new GameObject("CardCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Instantiate the appropriate card prefab based on card type
        GameObject cardPrefab = (cardType == CardType.Unit)
            ? (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Cards/UnitBase.prefab", typeof(GameObject))
            : (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Cards/SpellBase.prefab", typeof(GameObject));

        GameObject newCard = Instantiate(cardPrefab, canvasObject.transform); // Parent to the canvas

        // Set card values using updated paths
        if (cardType == CardType.Unit)
        {
            newCard.transform.Find(unitCardNameString).GetComponent<TextMeshProUGUI>().text = cardName;
            newCard.transform.Find(unitCardDescriptionString).GetComponent<TextMeshProUGUI>().text = cardDescription;
            newCard.transform.Find(unitCardAttackString).GetComponent<TextMeshProUGUI>().text = cardAttack.ToString();
            newCard.transform.Find(unitCardDefenseString).GetComponent<TextMeshProUGUI>().text = cardDefense.ToString();
            newCard.transform.Find(unitCardLevelString).GetComponent<TextMeshProUGUI>().text = cardLevel.ToString();
            newCard.transform.Find(unitCardTagsString).GetComponent<TextMeshProUGUI>().text = cardTags;
            newCard.transform.Find(unitCardImageString).GetComponent<Image>().sprite = cardImage;

            AdjustCardNameWidth(newCard.transform.Find(unitCardNameString).GetComponent<TextMeshProUGUI>(), cardName, maxNameWidth);
        }
        else if (cardType == CardType.Spell)
        {
            newCard.transform.Find(spellCardNameString).GetComponent<TextMeshProUGUI>().text = spellName;
            newCard.transform.Find(spellCardDescriptionString).GetComponent<TextMeshProUGUI>().text = spellDescription;
            newCard.transform.Find(spellCardTypeString).GetComponent<TextMeshProUGUI>().text = spellType.ToString();
            newCard.transform.Find(spellCardImageString).GetComponent<Image>().sprite = spellImage;

            // Check if the spell condition is empty and disable the condition image if so
            if (string.IsNullOrWhiteSpace(spellCondition))
            {
                newCard.transform.Find(spellCardConditionString).gameObject.SetActive(false); // Disable the condition field
            }
            else
            {
                newCard.transform.Find(spellCardConditionString).GetComponent<TextMeshProUGUI>().text = spellCondition;
            }

            AdjustCardNameWidth(newCard.transform.Find(spellCardNameString).GetComponent<TextMeshProUGUI>(), spellName, maxNameWidth);
        }

        // Save the card as a new prefab (optional)
        PrefabUtility.SaveAsPrefabAsset(newCard, $"Assets/Cards/{subFolder}/{(cardType == CardType.Unit ? cardName : spellName)}.prefab");

        // Adjust the size of the card based on your requirements
        RectTransform cardRectTransform = newCard.GetComponent<RectTransform>();
        cardRectTransform.sizeDelta = new Vector2(250, 350); // Set to your preferred dimensions

        // DestroyImmediate(newCard); // Uncomment this line if you want to destroy the card after creating the prefab.
        Debug.Log($"Card '{(cardType == CardType.Unit ? cardName : spellName)}' created successfully!");
    }


    void AdjustCardNameWidth(TextMeshProUGUI nameField, string cardName, float maxWidth)
    {
        if (nameField == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on the card object.");
            return;
        }

        // Set the text and disable auto sizing
        nameField.text = cardName;
        nameField.enableAutoSizing = false;
        Debug.Log($"Card Name set to: {cardName}");

        // Get the RectTransform and calculate the preferred width
        RectTransform rt = nameField.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("RectTransform component not found on TextMeshProUGUI.");
            return;
        }

        float preferredWidth = nameField.preferredWidth;
        Debug.Log($"Preferred Width for '{cardName}': {preferredWidth}");

        // Clamp width to maxWidth and set RectTransform width
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        Debug.Log($"Final Width for '{cardName}' set to: {preferredWidth}");

        float xScale = maxNameWidth / preferredWidth;
        if (xScale < 1)
        {
            rt.localScale = new Vector3(xScale, 1, 1);
            Debug.Log($"Scale applied to '{cardName}': {xScale}");
        }
    }
}
