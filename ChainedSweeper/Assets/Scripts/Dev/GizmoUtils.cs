using UnityEngine;

namespace DevTools
{
    public static class GizmoUtils
    {
        public static void DrawRect(Vector2 bottomLeft, Vector2 topRight)
        {
            var topLeft = new Vector3(bottomLeft.x, topRight.y, 0);
            var bottomRight = new Vector3(topRight.x, bottomLeft.y, 0);

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }

        public static void DrawRectCentered(Vector2 center, Vector2 size)
        {
            Vector2 bottomLeft = center - size / 2;
            Vector2 topRight = center + size / 2;
            DrawRect(bottomLeft, topRight);
        }

        public static void DrawRectCast(Vector2 pos, Vector2 dir, Vector2 size, float dist)
        {
            DrawRectCentered(pos, size);
            Gizmos.DrawLine(pos, pos + dist * dir);
            DrawRectCentered(pos + dist * dir, size);
        }
    }
}