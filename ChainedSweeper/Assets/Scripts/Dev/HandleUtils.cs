using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DevTools
{
    public static class HandleUtils
    {
        public static Color TransparentCornflowerBlue => new(0f, 0f, 1f, 0.25f);
        public static Color TransparentYellow => new(1f, 1f, 0f, 0.25f);
        public static Color TransparentRed => new(1f, 0f, 0f, 0.25f);
        public static Color TransparentGreen => new(0f, 1f, 0.2f, 0.15f);
        public static Color TransparentWhite => new(1f, 1f, 1f, 0.15f);

        public static void DrawArc(Vector3 center, float startAngularPos, float angularWidth, float radius,
            Color color)
        {
#if UNITY_EDITOR
            Handles.zTest = CompareFunction.LessEqual;

            var from = new Vector3(Mathf.Cos(startAngularPos * Mathf.Deg2Rad), 0f,
                Mathf.Sin(startAngularPos * Mathf.Deg2Rad));
            Handles.color = color;
            Handles.DrawSolidArc(center, Vector3.up, from, angularWidth, radius);
#endif
        }

        /// <summary>
        ///     Draws a radial rectangle slice
        /// </summary>
        /// <param name="angularPos"></param>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        /// <param name="height"></param>
        /// <param name="c"></param>
        public static void DrawWireRectSlice(
            float angularPos,
            float startRadius,
            float endRadius,
            float startY,
            float height,
            Color c)
        {
#if UNITY_EDITOR
            var angularTo = new Vector2(Mathf.Cos(angularPos * Mathf.Deg2Rad), Mathf.Sin(angularPos * Mathf.Deg2Rad));
            var points = new Vector3[4]
            {
                new(angularTo.x * startRadius, startY, angularTo.y * startRadius),
                new(angularTo.x * startRadius, startY + height, angularTo.y * startRadius),
                new(angularTo.x * endRadius, startY + height, angularTo.y * endRadius),
                new(angularTo.x * endRadius, startY, angularTo.y * endRadius)
            };
            Handles.DrawSolidRectangleWithOutline(points, c, c);
#endif
        }
    }
}