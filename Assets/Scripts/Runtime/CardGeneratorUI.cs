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

    // Reference to your existing generator script
    public VersalCardGenerator generator;

    private void Start()
    {
        if (Button_GenerateCards != null)
            Button_GenerateCards.onClick.AddListener(OnGenerateCardsClicked);

        if (Button_GenerateSheets != null)
            Button_GenerateSheets.onClick.AddListener(OnGenerateSheetsClicked);
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
}
