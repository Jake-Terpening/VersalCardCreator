using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class VersalCardGenerator
{
    private Vector2 cardSize = new Vector2(1125, 1575); // width x height in pixels
    private List<CardData> cardDataList = new List<CardData>();

    private GameObject unitCardPrefab;
    private GameObject spellCardPrefab;

    private RarityColorMap rarityColorMap;

    public void SetRarityColorMap(RarityColorMap map)
    {
        rarityColorMap = map;
    }


    public VersalCardGenerator(GameObject unitPrefab, GameObject spellPrefab, Vector2 cardSize)
    {
        this.unitCardPrefab = unitPrefab;
        this.spellCardPrefab = spellPrefab;
        this.cardSize = cardSize;
    }

    // Generate individual cards
    public void GenerateCards(string csvPath, string imageFolder, string outputFolder)
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSV file not found: " + csvPath);
            return;
        }

        ParseCSV(csvPath);

        foreach (var card in cardDataList)
        {
            GameObject prefab = (card.type.Equals("Unit", System.StringComparison.OrdinalIgnoreCase)) ? unitCardPrefab : spellCardPrefab;
            string imagePath = Path.Combine(imageFolder, card.name + ".png");
            Sprite sprite = LoadSprite(imagePath);

            RenderCardToPNG(prefab, card, sprite, Path.Combine(outputFolder, card.name + ".png"));
        }

        Debug.Log("Individual cards generated successfully!");
    }

    // Generate sheets (3x3 hardcoded)
    public void GenerateSheets(string csvPath, string imageFolder, string outputFolder)
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSV file not found: " + csvPath);
            return;
        }

        ParseCSV(csvPath);

        int cardsPerRow = 3;
        int cardsPerColumn = 3;

        int sheetWidth = cardsPerRow * (int)cardSize.x;
        int sheetHeight = cardsPerColumn * (int)cardSize.y;

        int sheetIndex = 0;
        Texture2D sheet = CreateEmptySheet(sheetWidth, sheetHeight);

        for (int i = 0; i < cardDataList.Count; i++)
        {
            int row = (i / cardsPerRow) % cardsPerColumn;
            int col = i % cardsPerRow;

            if (i > 0 && i % (cardsPerRow * cardsPerColumn) == 0)
            {
                SaveSheet(sheet, outputFolder, sheetIndex);
                sheetIndex++;

                sheet = CreateEmptySheet(sheetWidth, sheetHeight);
                row = 0;
                col = 0;
            }

            CardData card = cardDataList[i];
            GameObject prefab = (card.type.Equals("Unit", System.StringComparison.OrdinalIgnoreCase)) ? unitCardPrefab : spellCardPrefab;
            Sprite sprite = LoadSprite(Path.Combine(imageFolder, card.name + ".png"));
            Texture2D cardTex = RenderCardTexture(prefab, card, sprite);

            PasteCardIntoSheet(sheet, cardTex, col, row, cardsPerColumn);
            UnityEngine.Object.DestroyImmediate(cardTex);
        }

        SaveSheet(sheet, outputFolder, sheetIndex);
        Debug.Log("Card sheets generated successfully!");
    }

    // ---------------- Helper Functions ----------------

    private void ParseCSV(string csvPath)
    {
        cardDataList.Clear();
        string[] rows = File.ReadAllLines(csvPath);

        for (int i = 1; i < rows.Length; i++) // skip header row
        {
            List<string> cols = new List<string>();
            foreach (Match m in Regex.Matches(rows[i], @"(?:^|,)(?:""(?<val>[^""]*)""|(?<val>[^,]*))"))
            {
                cols.Add(m.Groups["val"].Value.Trim());
            }

            // Ensure we have at least 10 columns (Name, Level, Traits, Effect, Attack, Defense, Subtype, Condition, Rarity, Type)
            if (cols.Count < 10) continue;

            CardData card = new CardData
            {
                name = cols[0],
                level = cols[1],
                traits = cols[2].Replace(" ", ";"), // optional: convert spaces to semicolons
                effect = cols[3],
                attack = int.TryParse(cols[4], out int atk) ? atk : 0,
                defense = int.TryParse(cols[5], out int def) ? def : 0,
                subtype = cols[6],
                condition = cols[7],
                rarity = int.TryParse(cols[8], out int r) ? r : 1, // default rarity 1 if missing
                type = cols[9]
            };

            cardDataList.Add(card);
        }
    }



    private Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    private Texture2D RenderCardTexture(GameObject prefab, CardData card, Sprite sprite)
    {
        // Instantiate prefab
        GameObject instance = UnityEngine.Object.Instantiate(prefab);

        // Step 1: Setup Prefab
        SetupPrefab(instance, cardSize, sprite);

        // Step 2: Apply rarity colors
        ApplyRarityColors(instance, card);

        // Step 3: Assign text fields (with squish logic)
        AssignTextFields(instance, card);

        // Step 4: Setup temporary camera
        Camera cam = SetupCamera(cardSize);

        // Step 5: Render to texture
        Texture2D tex = RenderToTexture(cam, cardSize);

        // Step 6: Cleanup objects
        Cleanup(cam, instance);

        return tex;
    }

    private void SetupPrefab(GameObject instance, Vector2 cardSize, Sprite sprite)
    {
        // Ensure canvas exists
        Canvas canvas = instance.GetComponent<Canvas>() ?? instance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // RectTransform setup
        RectTransform rt = instance.GetComponent<RectTransform>() ?? instance.AddComponent<RectTransform>();
        rt.sizeDelta = cardSize;
        rt.pivot = new Vector2(0, 0);
        rt.localScale = Vector3.one;
        rt.position = Vector3.zero;

        // Assign main sprite (if any)
        UnityEngine.UI.Image img = instance.GetComponentInChildren<UnityEngine.UI.Image>();
        if (img != null && sprite != null)
            img.sprite = sprite;
    }

    private void ApplyRarityColors(GameObject instance, CardData card)
    {
        if (rarityColorMap == null) return;

        if (!rarityColorMap.TryGetColors(card.rarity, out RarityColor colors)) return;

        string baseObjectName;
        // Base background
        if (card.type.ToLower() == "unit")
        {
            baseObjectName = "UnitBase";
        }
        else
        {
            baseObjectName = "SpellBase";
        }

        Transform baseTransform = instance.transform.Find(baseObjectName);
        if (baseTransform != null && baseTransform.TryGetComponent(out UnityEngine.UI.Image baseImage))
            baseImage.color = colors.secondary;

        // Name box background
        Transform nameBackTransform = instance.transform.Find("NameTextBack");
        if (nameBackTransform != null && nameBackTransform.TryGetComponent(out UnityEngine.UI.Image nameBackImage))
            nameBackImage.color = colors.primary;
    }

    private void AssignTextFields(GameObject instance, CardData card)
    {
        TMPro.TextMeshProUGUI[] texts = instance.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (var t in texts)
        {
            

            string lowerName = t.name.ToLower();
            string fieldText = "";

            if (lowerName.Contains("name")) fieldText = card.name;
            else if (lowerName.Contains("description") || lowerName.Contains("effect")) fieldText = card.effect;
            else if (lowerName.Contains("attack")) fieldText = card.attack.ToString();
            else if (lowerName.Contains("defense")) fieldText = card.defense.ToString();
            else if (lowerName.Contains("level")) fieldText = card.level;
            else if (lowerName.Contains("condition")) fieldText = card.condition.ToString();
            else if (lowerName.Contains("tags") || lowerName.Contains("traits"))
                fieldText = string.Join(", ", card.traits.Split(';'));
            else if (lowerName.Contains("spelltype") || lowerName.Contains("subtype"))
                fieldText = card.subtype;
            else
            {
                Debug.Log($"Skipping '{t.name}' because it is not one of our set fields");
                continue;
            }

            t.text = fieldText;

            // Log assignment
            Debug.Log($"Assigning TMP '{t.name}' => '{fieldText}'");

            if (t.GetComponent<DoNotSquish>() != null)
            {
                Debug.Log($"Skipping '{t.name}' because it has DoNotSquish component.");
                continue;
            }
            // Squish width but preserve height
            float preferredWidth = t.preferredWidth;
            float maxWidth = t.rectTransform.rect.width;
            t.rectTransform.localScale = (preferredWidth > maxWidth) ? new Vector3(maxWidth / preferredWidth, 1f, 1f) : Vector3.one;
        }
    }


    private Camera SetupCamera(Vector2 cardSize)
    {
        Camera cam = new GameObject("TempCam").AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.orthographicSize = cardSize.y / 2f;
        cam.transform.position = new Vector3(cardSize.x / 2f, cardSize.y / 2f, -10f);
        return cam;
    }

    private Texture2D RenderToTexture(Camera cam, Vector2 cardSize)
    {
        RenderTexture rtTex = new RenderTexture((int)cardSize.x, (int)cardSize.y, 24);
        cam.targetTexture = rtTex;
        cam.Render();

        RenderTexture.active = rtTex;
        Texture2D tex = new Texture2D((int)cardSize.x, (int)cardSize.y, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rtTex.width, rtTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        cam.targetTexture = null;

        UnityEngine.Object.DestroyImmediate(rtTex);
        return tex;
    }

    private void Cleanup(Camera cam, GameObject instance)
    {
        UnityEngine.Object.DestroyImmediate(cam.gameObject);
        UnityEngine.Object.DestroyImmediate(instance);
    }



    private void RenderCardToPNG(GameObject prefab, CardData card, Sprite sprite, string path)
    {
        Texture2D tex = RenderCardTexture(prefab, card, sprite);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private Texture2D CreateEmptySheet(int width, int height)
    {
        Texture2D sheet = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] emptyPixels = new Color32[width * height];
        for (int i = 0; i < emptyPixels.Length; i++) emptyPixels[i] = new Color32(0, 0, 0, 0);
        sheet.SetPixels32(emptyPixels);
        return sheet;
    }

    private void PasteCardIntoSheet(Texture2D sheet, Texture2D cardTex, int col, int row, int cardsPerColumn)
    {
        for (int x = 0; x < cardTex.width; x++)
        {
            for (int y = 0; y < cardTex.height; y++)
            {
                Color pixel = cardTex.GetPixel(x, y);
                sheet.SetPixel(col * cardTex.width + x, (cardsPerColumn - 1 - row) * cardTex.height + y, pixel);
            }
        }
    }

    private void SaveSheet(Texture2D sheet, string folder, int index)
    {
        string path = Path.Combine(folder, "Sheet_" + (index + 1) + ".png");
        File.WriteAllBytes(path, sheet.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(sheet);
        Debug.Log("Saved sheet: " + path);
    }
}
