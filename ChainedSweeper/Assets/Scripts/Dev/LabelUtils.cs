using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DevTools
{
    public static class LabelUtils
    {
        private static GUIStyle labelStyle;

        public static void Label(Vector3 position, string text, Color c = default)
        {
#if UNITY_EDITOR
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.black;
                labelStyle.fontSize = 24;
                labelStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (c == default)
            {
                labelStyle.normal.textColor = Color.black;
            }
            else
            {
                labelStyle.normal.textColor = c;
            }

            Handles.Label(position, text, labelStyle);
#endif
        }
    }
}