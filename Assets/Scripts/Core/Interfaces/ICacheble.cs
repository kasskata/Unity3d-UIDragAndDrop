namespace Assets.Scripts.Core
{
    using UnityEngine;

    public interface ICacheble
    {
        RectTransform TransformCache { get; set; }

        bool IsCached { get; set; }

        void CacheInDragAndDropPanel();
    }
}