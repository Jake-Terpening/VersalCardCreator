using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CardBatchGenerator : EditorWindow
{
    public TextAsset cardCSV;              // Assign your CSV here
    public string outputFolder = "Assets/ExportedCards"; // Where PNGs will go
    public int exportWidth = 1050;         // Width in pixels for print (~300 DPI)
    public int exportHeight = 1470;        // Height in pixels for print (~3.5x5" card)
    
    [MenuItem("Tools/Batch Card Generator")]
    public static void ShowWindow()
    {
        GetWindow<CardBatchGenerator>("Batch Card Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Card Generator", EditorStyles.boldLabel);

        cardCSV = (TextAsset)EditorGUILayout.ObjectField("Card CSV", cardCSV, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        exportWidth = EditorGUILayout.IntField("Export Width", exportWidth);
        exportHeight = EditorGUILayout.IntField("Export Height", exportHeight);

        if (GUILayout.Button("Generate All Cards"))
        {
            if (cardCSV == null)
            {
                Debug.LogError("Please assign a CSV file!");
                return;
            }
            GenerateAllCards();
        }
    }

    private void GenerateAllCards()
    {
        // Parse CSV
        string[] rows = cardCSV.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length < 2)
        {
            Debug.LogError("CSV has no data rows.");
            return;
        }

        List<CardData> cardList = new List<CardData>();

        for (int i = 1; i < rows.Length; i++) // Skip header
        {
            string[] cols = rows[i].Split(',');
            if (cols.Length < 9) continue; // Skip malformed rows

            string name = cols[0].Trim();
            string type = cols[8].Trim();

            CardData cd = new CardData();
            cd.name = name;
            cd.type = type;

            if (type.Equals("Unit", System.StringComparison.OrdinalIgnoreCase))
            {
                cd.level = cols[1].Trim();
                cd.traits = cols[2].Trim();
                cd.effect = cols[3].Trim();
                int.TryParse(cols[4].Trim(), out cd.attack);
                int.TryParse(cols[5].Trim(), out cd.defense);
            }
            else if (type.Equals("Spell", System.StringComparison.OrdinalIgnoreCase))
            {
                cd.subtype = cols[6].Trim();
                cd.condition = cols[7].Trim();
                cd.effect = cols[3].Trim();
            }

            cardList.Add(cd);
        }

        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string parent = Path.GetDirectoryName(outputFolder);
            string newFolder = Path.GetFileName(outputFolder);
            AssetDatabase.CreateFolder(parent, newFolder);
        }

        // Loop through all cards
        foreach (CardData card in cardList)
        {
            //1️ Create prefab
            GameObject prefab = CreatePrefabFromData(card);

            //2️ Render PNG
            string path = Path.Combine(outputFolder, card.name + ".png");
            RenderPrefabToPNG(prefab, path, exportWidth, exportHeight);

            //3️ Cleanup prefab instance
            DestroyImmediate(prefab);
        }

        AssetDatabase.Refresh();
        Debug.Log("All cards generated!");
    }

    // Uses your CardImageExporter logic
    private GameObject CreatePrefabFromData(CardData data)
    {
        // Decide prefab path
        string basePath = (data.type == "Unit") ? "Assets/Cards/UnitBase.prefab" : "Assets/Cards/SpellBase.prefab";
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
        if (basePrefab == null)
        {
            Debug.LogError("Base prefab missing at: " + basePath);
            return null;
        }

        // Instantiate in the scene
        GameObject instance = Instantiate(basePrefab);

        // Assign fields (similar to CardImageExporter)
        var textFields = instance.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        var images = instance.GetComponentsInChildren<UnityEngine.UI.Image>(true);

        foreach (var tf in textFields)
        {
            if (tf.name.Contains("CardName")) tf.text = data.name;
            if (tf.name.Contains("CardDescription")) tf.text = data.effect;
            if (tf.name.Contains("CardLevel")) tf.text = data.level;
            if (tf.name.Contains("CardTags")) tf.text = data.traits;
            if (tf.name.Contains("CardAttack")) tf.text = data.attack.ToString();
            if (tf.name.Contains("CardDefense")) tf.text = data.defense.ToString();
            if (tf.name.Contains("CardSpellType")) tf.text = data.subtype;
            if (tf.name.Contains("CardCondition")) tf.text = data.condition;
        }

        return instance;
    }

    // Uses a RenderTexture to save prefab as PNG
    private void RenderPrefabToPNG(GameObject prefab, string filePath, int width, int height)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null. Cannot render PNG.");
            return;
        }

        // Create a temporary camera
        Camera cam = new GameObject("TempCam").AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear; // Transparent background

        // Get RectTransform of the prefab to fit the camera
        RectTransform rt = prefab.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Set camera size to match prefab height
            cam.orthographicSize = rt.rect.height / 2f;

            // Center camera on the prefab
            Vector3 prefabCenter = rt.position;
            cam.transform.position = new Vector3(prefabCenter.x, prefabCenter.y, -10f);
        }
        else
        {
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // Create RenderTexture with target resolution
        RenderTexture rtTex = new RenderTexture(width, height, 24);
        cam.targetTexture = rtTex;

        // Render the camera
        cam.Render();

        // Read pixels from the RenderTexture into a Texture2D
        RenderTexture.active = rtTex;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode to PNG and save
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        // Cleanup
        RenderTexture.active = null;
        cam.targetTexture = null;
        DestroyImmediate(cam.gameObject);
        DestroyImmediate(rtTex);
        DestroyImmediate(tex);

        Debug.Log("Exported PNG: " + filePath);
    }

}
