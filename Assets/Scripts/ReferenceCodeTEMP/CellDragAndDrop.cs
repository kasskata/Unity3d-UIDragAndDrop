using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Draged element raycast disabled to indicates drop zones")]
    public bool dropZonesRaycast = true;
    public bool lockIndexing = false;

    public UnityEvent onIndexChange;

    [HideInInspector]
    public Transform shiftedElement;

    [HideInInspector]
    public sbyte shiftDirection;

    [HideInInspector]
    public RectTransform dragTarget;

    [HideInInspector]
    public bool isDragging;

    private int index;
    private Transform content;
    private ScrollRect scrollRectCache;
    private Vector2 currentScrollPosition;
    private RectTransform scrollRectTransform;
    private RectTransform dummyPrefab;

    public int Index { get { return this.index; } }

    public Transform Content
    {
        get
        {
            if (this.content == null)
            {
                this.content = GetComponent<Transform>().GetChild(0);
            }

            return this.content;
        }
    }

    public ScrollRect ScrollRectCache
    {
        get
        {
            if (this.scrollRectCache == null)
            {
                this.scrollRectCache = GetComponent<ScrollRect>();
                this.scrollRectTransform = this.scrollRectCache.GetComponent<RectTransform>();
            }

            return this.scrollRectCache;
        }
    }

    public RectTransform DummyPrefab
    {
        get
        {
            if (this.dummyPrefab == null)
            {
                this.dummyPrefab = Instantiate(this.dragTarget.gameObject, this.Content).GetComponent<RectTransform>();

#if UNITY_EDITOR
                this.dummyPrefab.name = "Dummy";
#endif
                Component[] components = this.dummyPrefab.GetComponents<Component>();

                for (int i = components.Length - 1; i > 0; i--)
                {
                    Destroy(components[i]);
                }

                foreach (Transform child in this.dummyPrefab)
                {
                    Destroy(child.gameObject);
                }

                Utilities.DestroyChildren(this.dummyPrefab);
                AddMissingComponent<LayoutElement>(this.dummyPrefab.gameObject).ignoreLayout = true;
            }

            this.dummyPrefab.sizeDelta = this.dragTarget.GetComponent<RectTransform>().sizeDelta;
            return this.dummyPrefab;
        }
    }

    public void Awake()
    {
        if (Camera.main == null)
        {
            Debug.LogError("The camera on scene is not with tag \"MainCamera\"");
            return;
        }
        int i;
        for (i = 0; i < this.Content.childCount; i++)
        {
            AddHoldEvent((uint)i);
        }

    }

    public void Update()
    {
        if (this.isDragging)
        {
            if (!this.lockIndexing)
            {
                const float delimiter = -0.001f;

                bool isBiggerMinY = this.dragTarget.localPosition.y + this.Content.localPosition.y >=
                                    this.scrollRectTransform.rect.yMin;
                bool isBiggerMaxY = this.dragTarget.localPosition.y + this.Content.localPosition.y <=
                                    this.scrollRectTransform.rect.yMax;

                if (isBiggerMinY && isBiggerMaxY)
                {
                }
                else
                {
                    if (isBiggerMinY)
                    {
                        this.currentScrollPosition.y = Mathf.Clamp01(this.currentScrollPosition.y - delimiter);
                    }
                    else if (isBiggerMaxY)
                    {
                        this.currentScrollPosition.y = Mathf.Clamp01(this.currentScrollPosition.y + delimiter);
                    }
                }

            }

            this.ScrollRectCache.normalizedPosition = this.currentScrollPosition;
            this.dragTarget.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this.dragTarget != null && this.dropZonesRaycast)
        {
            CanvasGroup addMissingComponent = AddMissingComponent<CanvasGroup>(this.dragTarget.gameObject);
            addMissingComponent.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!this.lockIndexing)
        {
            if (this.dragTarget == null)
            {
                return;
            }

            //Set dummy object position to be in bounds.
            if (0 <= this.index - 1)
            {
                if (this.dragTarget.anchoredPosition.y >
                    this.Content.GetChild(this.index - 1).GetComponent<RectTransform>().anchoredPosition.y)
                {
                    this.shiftDirection = -1;
                    ShiftElement();
                }
            }

            if (this.index + 1 < this.Content.childCount - 1)
            {
                if (this.dragTarget.anchoredPosition.y <
                    this.Content.GetChild(this.index + 1).GetComponent<RectTransform>().anchoredPosition.y)
                {
                    this.shiftDirection = +1;
                    ShiftElement();
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDragging = false;
        this.shiftedElement = null;

        if (this.dragTarget == null)
        {
            return;
        }

        AddMissingComponent<LayoutElement>(this.dragTarget.gameObject).ignoreLayout = false;
        AddMissingComponent<LayoutElement>(this.DummyPrefab.gameObject).ignoreLayout = true;

        this.dragTarget.SetSiblingIndex(this.index);

        if (this.dropZonesRaycast)
        {
            CanvasGroup targetCanvasGroup = this.dragTarget.GetComponent<CanvasGroup>();
            if (targetCanvasGroup != null)
            {
                targetCanvasGroup.blocksRaycasts = true;
            }
        }

        this.dragTarget = null;

        this.dummyPrefab.SetParent(this.scrollRectTransform);
    }

    private void ShiftElement()
    {
        this.index += this.shiftDirection;
        this.shiftedElement = this.Content.GetChild(this.index);
        this.DummyPrefab.SetSiblingIndex(this.index);
        this.onIndexChange.Invoke();
    }

    private void AddHoldEvent(uint i)
    {
        GameObject go = this.Content.GetChild((int)i).gameObject;
        HoldButton onHoldComponent = AddMissingComponent<HoldButton>(go);
        if (onHoldComponent.onHold == null)
        {
            onHoldComponent.onHold = new UnityEvent();
        }

        onHoldComponent.onHold.AddListener(() =>
            TakeDraggable(go.GetComponent<RectTransform>()));
    }

    public T Add<T>(T cellDragAndDropPrefab) where T : MonoBehaviour
    {
        T child = Utilities.AddChild(this.Content, cellDragAndDropPrefab);
        child.transform.SetParent(this.Content);
        child.transform.SetAsLastSibling();
        AddHoldEvent((uint)child.transform.GetSiblingIndex());
        return child;
    }

    public T Add<T>(T cellDragAndDropPrefab, int siblingIndex) where T : MonoBehaviour
    {
        T child = Utilities.AddChild(this.Content, cellDragAndDropPrefab);
        child.transform.SetParent(this.Content);
        child.transform.SetSiblingIndex(siblingIndex);
        AddHoldEvent((uint)siblingIndex);
        return child;
    }

    public void RemoveAt(uint indexChild)
    {
        Transform child = this.Content.GetChild((int)indexChild);
        child.SetParent(null);
        Destroy(child.gameObject);
    }

    private void TakeDraggable(RectTransform selectedTransform)
    {
        this.dragTarget = selectedTransform;
        this.index = this.dragTarget.GetSiblingIndex();
        AddMissingComponent<LayoutElement>(this.dragTarget.gameObject).ignoreLayout = true;
        AddMissingComponent<LayoutElement>(this.DummyPrefab.gameObject).ignoreLayout = false;

        this.dragTarget.SetAsLastSibling();

        this.dummyPrefab.SetParent(this.Content);
        this.DummyPrefab.SetSiblingIndex(this.index);

        this.currentScrollPosition = this.ScrollRectCache.normalizedPosition;
        this.isDragging = true;
    }

    public static T AddMissingComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();

        if (component == null)
        {
            component = go.AddComponent<T>();
        }

        return component;
    }
}