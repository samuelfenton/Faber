using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomDebug : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    public const float DEFAULT_LINE_THICKNESS = 5.0f;
    public const float DEFAULT_LINE_THICKNESS_HALF = DEFAULT_LINE_THICKNESS/2.0f;

    public static void DrawSquare(Vector3 p_center, float p_width, float p_height, Vector3 p_forward, Color p_color, float p_thickness = DEFAULT_LINE_THICKNESS)
    {
        float halfWidth = p_width / 2.0f;
        float halfHeight = p_height / 2.0f;

        Vector3 toRightOffset = p_forward * halfWidth;
        Vector3 toTopOffset = Vector3.up * halfHeight;

        Vector3 topRight = p_center + toRightOffset + toTopOffset;
        Vector3 topLeft = p_center - toRightOffset + toTopOffset;
        Vector3 bottomRight = p_center + toRightOffset - toTopOffset;
        Vector3 bottomLeft = p_center - toRightOffset - toTopOffset;

        Handles.DrawBezier(topRight, bottomRight, topRight, bottomRight, p_color, null, p_thickness);
        Handles.DrawBezier(bottomRight, bottomLeft, bottomRight, bottomLeft, p_color, null, p_thickness);
        Handles.DrawBezier(bottomLeft, topLeft, bottomLeft, topLeft, p_color, null, p_thickness);
        Handles.DrawBezier(topLeft, topRight, topLeft, topRight, p_color, null, p_thickness);

    }
#endif
}
