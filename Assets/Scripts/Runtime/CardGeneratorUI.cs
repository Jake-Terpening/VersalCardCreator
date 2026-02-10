using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CardGeneratorUI : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField InputField_CSV;
    public TMP_InputField InputField_ImageFolder;
    public TMP_InputField InputField_OutputFolder;

    [Header("Buttons")]
    public Button Button_GenerateCards;
    public Button Button_GenerateSheets;

    [Header("Prefabs")]
    [SerializeField] private GameObject unitCardPrefab;
    [SerializeField] private GameObject spellCardPrefab;

    [Header("CardSize")]
    [SerializeField]
    private float width = 500f;
    [SerializeField]
    private float height = 700f;

    [Header("Rarity Colors")]
    public RarityColorMap rarityColorMap;

    // Reference to your existing generator script
    public VersalCardGenerator generator;

    private void Start()
    {
        if (Button_GenerateCards != null)
            Button_GenerateCards.onClick.AddListener(OnGenerateCardsClicked);

        if (Button_GenerateSheets != null)
            Button_GenerateSheets.onClick.AddListener(OnGenerateSheetsClicked);

        generator = new VersalCardGenerator(unitCardPrefab, spellCardPrefab, new Vector2(width, height));
        generator.SetRarityColorMap(rarityColorMap);

    }

    private void OnGenerateCardsClicked()
    {
        if (!ValidatePaths()) return;

        generator.GenerateCards(
            csvPath: InputField_CSV.text,
            imageFolder: InputField_ImageFolder.text,
            outputFolder: InputField_OutputFolder.text
        );

        Debug.Log("Individual cards generated successfully!");
    }

    private void OnGenerateSheetsClicked()
    {
        if (!ValidatePaths()) return;

        generator.GenerateSheets(
            csvPath: InputField_CSV.text,
            imageFolder: InputField_ImageFolder.text,
            outputFolder: InputField_OutputFolder.text
        );

        Debug.Log("Card sheets generated successfully!");
    }

    private bool ValidatePaths()
    {
        if (string.IsNullOrEmpty(InputField_CSV.text) || !File.Exists(InputField_CSV.text))
        {
            Debug.LogError("CSV file path is invalid!");
            return false;
        }

        if (string.IsNullOrEmpty(InputField_ImageFolder.text) || !Directory.Exists(InputField_ImageFolder.text))
        {
            Debug.LogError("Image folder path is invalid!");
            return false;
        }

        if (string.IsNullOrEmpty(InputField_OutputFolder.text) || !Directory.Exists(InputField_OutputFolder.text))
        {
            Debug.LogError("Output folder path is invalid!");
            return false;
        }

        return true;
    }

    public void OnPickCSVClicked()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select CSV File", "", "csv", false);
        if (paths.Length > 0)
            InputField_CSV.text = paths[0];
    }

    public void OnPickImageFolderClicked()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Select Image Folder", "", false);
        if (paths.Length > 0)
            InputField_ImageFolder.text = paths[0];
    }

    public void OnPickOutputFolderClicked()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Select Output Folder", "", false);
        if (paths.Length > 0)
            InputField_OutputFolder.text = paths[0];
    }

}
