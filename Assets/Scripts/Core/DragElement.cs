namespace Assets.Scripts.Core
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using System;
    using UnityEngine.UI;

    public class DragElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [HideInInspector]
        public RectTransform transformCache;

        public Action OnBeginDragCallback { get; set; }
        public Action OnDragCallback { get; set; }
        public Action OnEndDragCallback { get; set; }

        public Vector2 LastPosition => this.lastPosition;

        private Vector2 lastPosition;
        public Transform LastParent { get; private set; }

        public bool IsDragging { get; private set; }

        public void Awake()
        {
            this.transformCache = this.transform.GetComponent<RectTransform>();
            this.LastParent = this.transformCache.parent;
        }


        public void Update()
        {
            if (this.IsDragging)
            {
                this.transformCache.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            this.IsDragging = true;
            this.lastPosition = this.transformCache.position;
            this.GetComponent<Graphic>().raycastTarget = false;

            this.OnBeginDragCallback?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.OnDragCallback?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            this.IsDragging = false;
            this.GetComponent<Graphic>().raycastTarget = true;

            this.OnEndDragCallback?.Invoke();
        }

        public void ReturnToLastPosition()
        {
            this.transformCache.position = this.lastPosition;
        }

        public void ReturnToLastParent()
        {
            this.transformCache.SetParent(this.LastParent);
        }

        public void SetLastParent()
        {
            this.LastParent = this.transformCache.parent;
        }
    }
}
