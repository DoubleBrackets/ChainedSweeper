using UnityEngine;

namespace Utils
{
    public static class UIExtensions
    {
        public static void SetVisible(this CanvasGroup group, bool visible)
        {
            group.alpha = visible ? 1 : 0;
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }
    }
}