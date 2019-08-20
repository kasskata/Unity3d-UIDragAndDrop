namespace Assets.Scripts.Core
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class DragAndDrop : MonoBehaviour
    {
        public Dictionary<int, DragElement> DragElements { get; private set; } = new Dictionary<int, DragElement>();

        public Dictionary<int, DropObject> DropObjects { get; private set; } = new Dictionary<int, DropObject>();

        public DragElement SelectedDragElement { get; set; }

        public DragElement LastSelectedDragElement { get; set; }

        public DropObject HoveredDropObject { get; set; }

        public DropObject LastDropObject { get; set; }

        public UnityEvent onBeginDrag = new UnityEvent();
        public UnityEvent onDrag = new UnityEvent();
        public UnityEvent onEndDrag = new UnityEvent();
        public UnityEvent onDropElementOutside = new UnityEvent();

        public void Start()
        {
            DropObject[] dropObjects = this.transform.root.GetComponentsInChildren<DropObject>();
            for (int i = 0; i < dropObjects.Length; i++)
            {
                DropObject dropObject = dropObjects[i];
                CacheDropObject(dropObject);
            }

            DragElement[] childrenDragElements = this.transform.root.GetComponentsInChildren<DragElement>();
            for (int i = 0; i < childrenDragElements.Length; i++)
            {
                CacheDragElement(childrenDragElements[i]);
            }
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (this.transform.root.GetComponentsInChildren<DragAndDrop>().Length > 1)
            {
                DragAndDropEditor.Log("More than one DragAndDrop instance has detected. You may have logical problems. Please centralize DragAndDrop logic on one place.", DragAndDropEditor.LogType.Warning, this.gameObject);
            }
        }
#endif
        public DragElement AddDragElement(Transform parent, string name = "NewDragElement")
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(DragElement));
            go.transform.SetParent(parent);
            DragElement dragElement = go.GetComponent<DragElement>();
            CacheDragElement(dragElement);

            return dragElement;
        }

        public DropObject AddDropObject(Transform parent, string name = "NewDropObject")
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(DropObject), typeof(Image));
            go.transform.SetParent(parent);
            go.GetComponent<Image>().color = Color.magenta;
            DropObject dropObject = go.GetComponent<DropObject>();
            dropObject.transform.localScale = Vector2.one;
            CacheDropObject(dropObject);

            return dropObject;
        }

        private void CacheDropObject(DropObject dropObject)
        {
            PrepareDropEvents(dropObject);

            int instanceId = dropObject.GetInstanceID();
            if (!this.DropObjects.ContainsKey(instanceId))
            {
                this.DropObjects.Add(instanceId, null);
            }

            this.DropObjects[instanceId] = dropObject;
        }

        public void CacheDragElement(DragElement dragElement)
        {
            PrepareItemEvents(dragElement);

            int instanceId = dragElement.GetInstanceID();
            if (!this.DragElements.ContainsKey(instanceId))
            {
                this.DragElements.Add(instanceId, null);
            }

            this.DragElements[instanceId] = dragElement;
        }

        private void PrepareItemEvents(DragElement dragElement)
        {
            dragElement.OnBeginDragCallback = () =>
            {
                this.SelectedDragElement = dragElement;
                this.SelectedDragElement.transformCache.SetParent(this.transform);
                //Debug.Log($"Begin Drag: {this.SelectedDragElement} D&D: {this.name}");

                if (this.HoveredDropObject != null)
                {
                    this.LastDropObject = this.HoveredDropObject;
                }

                this.onBeginDrag.Invoke();
            };

            dragElement.OnDragCallback = () =>
            {
                this.onDrag.Invoke();
            };

            dragElement.OnEndDragCallback = () =>
            {
                this.onEndDrag.Invoke();

                //Debug.Log($"End Drag : {this.SelectedDragElement} D&D: {this.name}");
                if (this.HoveredDropObject != null)
                {
                    if (this.HoveredDropObject.IsEmpty)
                    {
                        this.SelectedDragElement.transformCache.SetParent(this.HoveredDropObject.transformCache);
                        this.SelectedDragElement.transformCache.localPosition = Vector2.zero;
                        this.SelectedDragElement.SetLastParent();
                        this.HoveredDropObject.IsEmpty = false;

                        if (this.LastDropObject != null)
                        {
                            this.LastDropObject.IsEmpty = true;
                        }
                    }
                    else
                    {
                        this.SelectedDragElement.ReturnToLastParent();

                        if (this.LastDropObject)
                        {
                            this.LastDropObject.IsEmpty = false;
                        }
                    }
                }
                else if (this.HoveredDropObject == null)
                {
                    this.onDropElementOutside.Invoke();
                    if (this.LastDropObject)
                    {
                        this.LastDropObject.IsEmpty = true;
                    }
                }

                this.LastSelectedDragElement = this.SelectedDragElement;
                this.SelectedDragElement = null;
            };
        }

        private void PrepareDropEvents(DropObject dropObject)
        {
            dropObject.OnPointerEnterCallback = () =>
            {
                this.HoveredDropObject = dropObject;
                //Debug.Log("Enter" + this.HoveredDropObject);
            };

            dropObject.OnPointerExitCallback = () =>
            {
                this.HoveredDropObject = null;
                //                Debug.Log(" Exit " + this.HoveredDropObject);
            };
        }

        public static T AddMissingComponent<T>(GameObject go) where T : Component
        {
            T found = go.GetComponent<T>();

            if (found == null)
            {
                return go.AddComponent<T>();
            }

            return found;
        }
    }

    public static class DragAndDropEditor
    {
        private const string DragAndDropString = "[DragAndDrop]: ";

#if UNITY_EDITOR
        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        [SerializeField]
        public static bool enableLogs = true;

        [MenuItem("GameObject/DragAndDrop/CreateDragAndDrop %DO", false)]
        [MenuItem("DragAndDrop/CreateDragAndDrop %DD", false)]
        public static void CreateDragAndDropOnSelected()
        {
            GameObject go = new GameObject("DragAndDropPanel", typeof(RectTransform), typeof(DragAndDrop));
            go.transform.SetParent(Selection.activeGameObject.transform);

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.localScale = Vector2.one;
            rectTransform.sizeDelta = new Vector2(400, 200);

            DragAndDrop dragAndDrop = go.GetComponent<DragAndDrop>();

            GameObject dropObjectContainer = new GameObject("DropObjectContainer", typeof(GridLayoutGroup));
            dropObjectContainer.transform.SetParent(dragAndDrop.transform);
            rectTransform = dropObjectContainer.GetComponent<RectTransform>();
            rectTransform.localScale = Vector2.one;
            rectTransform.sizeDelta = new Vector2(400, 200);


            DragElement dragElement = dragAndDrop.AddDragElement(dragAndDrop.transform);
            dragElement.transform.localScale = Vector2.one;
            dragElement.transform.localPosition += new Vector3(0, -200, 0);
            Image image = DragAndDrop.AddMissingComponent<Image>(dragElement.gameObject);
            image.color = Color.cyan;


            DropObject dropObject = dragAndDrop.AddDropObject(dropObjectContainer.transform);
            dropObject.transform.localScale = Vector2.one;
            image = DragAndDrop.AddMissingComponent<Image>(dropObject.gameObject);
            image.color = Color.magenta;

            Undo.RegisterCreatedObjectUndo(go, "Create DragAndDrop");
        }

        [MenuItem("GameObject/DragAndDrop/CreateDragElement %DO", false)]
        [MenuItem("DragAndDrop/CreateDragElement %DE", false)]
        [MenuItem("CONTEXT/DragAndDrop/CreateDragElement %DE", false)]
        public static void AddDragElementOnSelected()
        {
            DragAndDrop dragAndDrop = Selection.activeGameObject.GetComponent<DragAndDrop>();
            if (dragAndDrop == null)
            {
                Log("Selected GameObject do not have Drag And Drop Component. Please assign and try again", LogType.Error, Selection.activeGameObject);
                return;
            }

            DragElement dragElement = dragAndDrop.AddDragElement(Selection.activeGameObject.transform);
            Undo.RegisterCreatedObjectUndo(dragElement, "Create DragElement");
        }

        [MenuItem("GameObject/DragAndDrop/CreateDropObject %DO", false)]
        [MenuItem("DragAndDrop/CreateDropObject %DO", false)]
        [MenuItem("CONTEXT/DragAndDrop/CreateDropObject %DO", false)]
        public static void AddDropObjectOnSelected()
        {
            DragAndDrop dragAndDrop = Selection.activeGameObject.GetComponent<DragAndDrop>();
            if (dragAndDrop == null)
            {
                Log("Selected GameObject do not have Drag And Drop Component. Please assign and try again", LogType.Error, Selection.activeGameObject);
            }

            DropObject dropObject = dragAndDrop.AddDropObject(Selection.activeGameObject.transform);
            Undo.RegisterCreatedObjectUndo(dropObject, "Create DropObject");
        }

        public static void Log(string message, LogType logType, GameObject go)
        {
            if (!enableLogs)
                return;

            switch (logType)
            {
                case LogType.Warning:
                    Debug.LogWarning($"{DragAndDropString}{message}", go);
                    break;
                case LogType.Error:
                    Debug.LogError($"{DragAndDropString}{message}", go);
                    break;
                default:
                    Debug.Log($"{DragAndDropString}{message}", go);
                    break;
            }
        }
#endif
    }
}
