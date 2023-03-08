using UnityEngine;
using System.Collections;

public static class ColorExtentions
{
    public static Color32 SetColor32Alpha(this Color32 color, float alpha)
    {
        color.a = (byte)alpha;
        return color;
    }

    public static Color SetColorAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
