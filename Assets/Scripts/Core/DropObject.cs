namespace Assets.Scripts.Core
{
    using UnityEngine;
    using System;
    using UnityEngine.EventSystems;

    public class DropObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action OnPointerEnterCallback { get; set; }
        public Action OnPointerExitCallback { get; set; }

        public bool isEmpty;

        [SerializeField]
        public bool IsEmpty
        {
            get => this.isEmpty;
            set => this.isEmpty = value;
        }

        [HideInInspector]
        public RectTransform transformCache;

        public void Start()
        {
            this.transformCache = this.transform.GetComponent<RectTransform>();
            this.isEmpty = this.GetComponentInChildren<DragElement>() == null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.OnPointerEnterCallback?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.OnPointerExitCallback?.Invoke();
        }
    }
}
