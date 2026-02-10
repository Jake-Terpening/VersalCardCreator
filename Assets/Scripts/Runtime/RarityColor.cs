using UnityEngine;

[System.Serializable]
public struct RarityColor
{
    public Color primary;
    public Color secondary;

    public RarityColor(Color primaryColor, Color secondaryColor)
    {
        primary = primaryColor;
        secondary = secondaryColor;
    }
}
