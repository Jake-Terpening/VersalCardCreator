using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CardExporter : EditorWindow
{
    private GameObject cardPrefab; // Reference to your card prefab

    [MenuItem("Tools/Card Exporter")]
    public static void ShowWindow()
    {
        GetWindow<CardExporter>("Card Exporter");
    }

    private void OnGUI()
    {
        cardPrefab = (GameObject)EditorGUILayout.ObjectField("Card Prefab", cardPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Export Card as Image"))
        {
            ExportCard();
        }
    }

    void ExportCard()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned!");
            return;
        }

        // Create an instance of the card
        GameObject cardInstance = Instantiate(cardPrefab);
        Debug.Log("Card instance created.");

        // Set up the camera
        Camera camera = new GameObject("CardCamera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear; // Set background to transparent

        // Set the position and size to match the card
        RectTransform rt = cardInstance.GetComponent<RectTransform>();
        float cardWidth = rt.rect.width;
        float cardHeight = rt.rect.height;

        camera.orthographicSize = cardHeight / 2; // Set camera size to half the card height
        camera.transform.position = new Vector3(rt.position.x, rt.position.y, -10); // Position camera behind the card
        Debug.Log($"Camera set at position: {camera.transform.position} with orthographic size: {camera.orthographicSize}");

        // Render to texture
        RenderTexture renderTexture = new RenderTexture((int)cardWidth, (int)cardHeight, 24);
        camera.targetTexture = renderTexture;
        Debug.Log($"RenderTexture created with dimensions: {cardWidth}x{cardHeight}");

        // Allow the camera to render
        camera.Render();
        Debug.Log("Camera rendered the card.");

        // Read the pixels from the RenderTexture and create a Texture2D
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D((int)cardWidth, (int)cardHeight, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        Debug.Log($"Texture2D created from RenderTexture with dimensions {renderTexture.width},{renderTexture.height}");

        // Save as PNG
        string path = EditorUtility.SaveFilePanel("Save Card Image", "", "CardImage.png", "png");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("File save path is invalid or canceled.");
            return;
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log("Card image saved to: " + path);

        
        // Clean up
        RenderTexture.active = null;
        camera.targetTexture = null;  // Clear the target texture before destroying it
        DestroyImmediate(renderTexture);
        DestroyImmediate(camera.gameObject);
        DestroyImmediate(cardInstance);
        Debug.Log("Cleaned up resources.");
        
    }
}
