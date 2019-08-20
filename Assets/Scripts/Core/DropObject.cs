namespace Assets.Scripts.Core
{
    using UnityEngine;
    using System;
    using UnityEngine.EventSystems;

    public class DropObject : MonoBehaviour, ICacheble, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Check whether when this DropObject have DragElement inside.
        /// </summary>
        public bool isEmpty = true;

        /// <summary>
        /// DropObject Transform cached reference. Workaround GetComponent method for better performance.
        /// </summary>
        public RectTransform TransformCache { get; set; }

        /// <summary>
        /// Unique Id for this DropObject.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Check whether when that DropObject is already cached in DragAndDrop collections and do not check it again.
        /// </summary>
        public bool IsCached { get; set; }

        /// <summary>
        /// Official event link callback for DragAndDrop when pointer/touch enter on that object. Do not rewrite because break main logic. 
        /// </summary>
        [HideInInspector]
        public Action OnPointerEnterCallback { get; set; }

        /// <summary>
        /// Official event link callback for DragAndDrop when pointer/touch exit from that object. Do not rewrite because will break main logic!
        /// </summary>
        [HideInInspector]
        public Action OnPointerExitCallback { get; set; }

        public void Awake()
        {
            this.Id = Guid.NewGuid();
            this.TransformCache = this.transform.GetComponent<RectTransform>();
        }

        public void Start()
        {
            this.isEmpty = this.GetComponentInChildren<DragElement>() == null;
            CacheInDragAndDropPanel();
        }

        public void OnValidate()
        {
            this.isEmpty = this.GetComponentInChildren<DragElement>() == null;
        }

        /// <summary>
        /// Cache in main DragAndDrop <see cref="DragAndDrop.DropObjectsCache"/> when is not cached.
        /// </summary>
        public void CacheInDragAndDropPanel()
        {
            if (!this.IsCached)
            {
                DragAndDrop dragAndDrop = this.TransformCache.root.GetComponentInChildren<DragAndDrop>();
                dragAndDrop.CacheDropObject(this);
            }
        }

        /// <summary>
        /// On pointer/touch enter event.
        /// </summary>
        /// <param name="eventData">Unused. Derived from Interface.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            this.OnPointerEnterCallback?.Invoke();
        }

        /// <summary>
        /// On pointer/touch exit event.
        /// </summary>
        /// <param name="eventData">Unused. Derived from Interface.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            this.OnPointerExitCallback?.Invoke();
        }
    }
}
