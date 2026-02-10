using UnityEditor;
using UnityEngine;
using System.IO;

public class CardSheetGenerator : EditorWindow
{
    public string inputFolder = "Assets/ExportedCards"; // Where your card PNGs are
    public string outputFolder = "Assets/CardSheets";   // Where to save sheets
    public int cardsPerRow = 3;
    public int cardsPerColumn = 3;
    public int cardWidth = 1125;   // Should match batch export
    public int cardHeight = 1575;

    [MenuItem("Tools/Card Sheet Generator")]
    public static void ShowWindow()
    {
        GetWindow<CardSheetGenerator>("Card Sheet Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Card Sheet Generator", EditorStyles.boldLabel);

        inputFolder = EditorGUILayout.TextField("Input Folder", inputFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        cardsPerRow = EditorGUILayout.IntField("Cards Per Row", cardsPerRow);
        cardsPerColumn = EditorGUILayout.IntField("Cards Per Column", cardsPerColumn);
        cardWidth = EditorGUILayout.IntField("Card Width", cardWidth);
        cardHeight = EditorGUILayout.IntField("Card Height", cardHeight);

        if (GUILayout.Button("Generate Sheets"))
        {
            GenerateSheets();
        }
    }

    private void GenerateSheets()
    {
        if (!Directory.Exists(inputFolder))
        {
            Debug.LogError("Input folder does not exist: " + inputFolder);
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string[] files = Directory.GetFiles(inputFolder, "*.png");
        if (files.Length == 0)
        {
            Debug.LogError("No PNG files found in input folder.");
            return;
        }

        int cardsPerSheet = cardsPerRow * cardsPerColumn;
        int sheetCount = Mathf.CeilToInt((float)files.Length / cardsPerSheet);

        for (int s = 0; s < sheetCount; s++)
        {
            Texture2D sheet = new Texture2D(cardWidth * cardsPerRow, cardHeight * cardsPerColumn, TextureFormat.RGBA32, false);

            for (int y = 0; y < cardsPerColumn; y++)
            {
                for (int x = 0; x < cardsPerRow; x++)
                {
                    int index = s * cardsPerSheet + y * cardsPerRow + x;
                    if (index >= files.Length) break;

                    byte[] pngData = File.ReadAllBytes(files[index]);
                    Texture2D cardTex = new Texture2D(2, 2);
                    cardTex.LoadImage(pngData);

                    // Place card on sheet
                    for (int i = 0; i < cardTex.width; i++)
                    {
                        for (int j = 0; j < cardTex.height; j++)
                        {
                            Color pixel = cardTex.GetPixel(i, j);
                            sheet.SetPixel(x * cardWidth + i, (cardsPerColumn - 1 - y) * cardHeight + j, pixel);
                        }
                    }

                    DestroyImmediate(cardTex);
                }
            }

            sheet.Apply();

            string sheetPath = Path.Combine(outputFolder, "Sheet_" + (s + 1) + ".png");
            File.WriteAllBytes(sheetPath, sheet.EncodeToPNG());
            DestroyImmediate(sheet);

            Debug.Log("Generated sheet: " + sheetPath);
        }

        AssetDatabase.Refresh();
        Debug.Log("All sheets generated successfully.");
    }
}
