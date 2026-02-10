using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class VersalCardGenerator
{
    private Vector2 cardSize = new Vector2(1125, 1575); // width x height in pixels
    private List<CardData> cardDataList = new List<CardData>();

    private GameObject unitCardPrefab;
    private GameObject spellCardPrefab;

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

        for (int i = 1; i < rows.Length; i++)
        {
            string[] cols = rows[i].Split(',');
            if (cols.Length < 9) continue;

            CardData card = new CardData
            {
                name = cols[0].Trim(),
                level = cols[1].Trim(),
                traits = cols[2].Trim(),
                effect = cols[3].Trim(),
                attack = int.TryParse(cols[4], out int atk) ? atk : 0,
                defense = int.TryParse(cols[5], out int def) ? def : 0,
                subtype = cols[6].Trim(),
                condition = cols[7].Trim(),
                type = cols[8].Trim()
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
        GameObject instance = UnityEngine.Object.Instantiate(prefab);
        RectTransform rt = instance.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = cardSize;

        // Image assignment
        UnityEngine.UI.Image img = instance.GetComponentInChildren<UnityEngine.UI.Image>();
        if (img != null && sprite != null) img.sprite = sprite;

        // Text assignment
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
            else if (lowerName.Contains("tags") || lowerName.Contains("traits"))
                fieldText = string.Join(", ", card.traits.Split(';'));

            t.text = fieldText;

            t.enableAutoSizing = false;
            t.enableWordWrapping = false;

            float preferredWidth = t.preferredWidth;
            float maxWidth = t.rectTransform.rect.width;

            t.rectTransform.localScale = (preferredWidth > maxWidth) ? new Vector3(maxWidth / preferredWidth, 1f, 1f) : Vector3.one;
        }

        // Camera setup
        Camera cam = new GameObject("TempCam").AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.orthographicSize = cardSize.y / 2f;
        cam.transform.position = new Vector3(cardSize.x / 2f, cardSize.y / 2f, -10);

        RenderTexture rtTex = new RenderTexture((int)cardSize.x, (int)cardSize.y, 24);
        cam.targetTexture = rtTex;
        cam.Render();

        RenderTexture.active = rtTex;
        Texture2D tex = new Texture2D((int)cardSize.x, (int)cardSize.y, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rtTex.width, rtTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        cam.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(cam.gameObject);
        UnityEngine.Object.DestroyImmediate(rtTex);
        UnityEngine.Object.DestroyImmediate(instance);

        return tex;
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
