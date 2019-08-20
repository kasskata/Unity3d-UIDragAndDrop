namespace Assets.Scripts.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    /// <summary>
    /// DragAndDrop orchestrate all DragElement and DropObject instances on current scene. That instances can be dynamically created through play mode and in Editor mode.
    /// </summary>
    public class DragAndDrop : MonoBehaviour
    {
        private const string DragAndDropString = "[DragAndDrop]: ";

        /// <summary>
        /// Show/Hide logs in editor only. 
        /// </summary>
        public bool debugLogs;

        /// <summary>
        /// DragElements shown in Unity inspector to watch automatically assigned DragElements from scene.
        /// </summary>
        public DragElement[] dragElements;

        /// <summary>
        /// DragElements shown in Unity inspector to watch automatically assigned DropObjects from scene.
        /// </summary>
        public DropObject[] dropObjects;

        private static bool enableLogs;

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement is begin drag on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onBeginDrag = new UnityEvent();

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement is being dragged on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onDrag = new UnityEvent();

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement was dropped on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onEndDrag = new UnityEvent();

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement was dropped outside on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onDropElementOutside = new UnityEvent();

        /// <summary>
        /// Automatically assigned DragElements from scene presented in cache mode with fast and easy way to get DragElement from the collection.
        /// </summary>
        public Dictionary<Guid, DragElement> DragElementsCache { get; private set; } = new Dictionary<Guid, DragElement>();

        /// <summary>
        /// Automatically assigned DropObjects from scene presented in cache mode with fast and easy way to get DropObjects from the collection.
        /// </summary>
        public Dictionary<Guid, DropObject> DropObjectsCache { get; private set; } = new Dictionary<Guid, DropObject>();

        /// <summary>
        /// Current DragElement witch selected on moment on begin drag event to that moment when drop it.
        /// </summary>
        public DragElement SelectedDragElement { get; private set; }

        /// <summary>
        /// Last DragElement witch selected before new selected DragElement.
        /// </summary>
        public DragElement LastSelectedDragElement { get; private set; }

        /// <summary>
        /// Current hovered (pointer/touch enter boundaries) DropObject.
        /// </summary>
        public DropObject HoveredDropObject { get; private set; }

        /// <summary>
        /// Last hovered (pointer/touch enter boundaries) DropObject before new Hovered DropObject.
        /// </summary>
        public DropObject LastDropObject { get; private set; }

        public void Start()
        {
            CacheDragElements();
            CacheDropObjects();
        }

        /// <summary>
        /// Add new GameObject with attached DragElement on specific place in hierarchy.
        /// </summary>
        /// <param name="parent">Parent for new GameObject.</param>
        /// <param name="prefab">Original object to copy.</param>
        /// <param name="name">Specific name for new Game object.</param>
        /// <returns>Created instance of DragElement.</returns>
        public DragElement AddDragElement(Transform parent, DragElement prefab = null, string name = "NewDragElement")
        {
            DragElement dragElement;
            if (prefab == null)
            {
                dragElement = new GameObject(name, typeof(RectTransform), typeof(DragElement)).GetComponent<DragElement>();
            }
            else
            {
                dragElement = Instantiate(prefab, parent);
                dragElement.name = name;
            }

            dragElement.transform.SetParent(parent);
            CacheDragElement(dragElement);

            return dragElement;
        }

        /// <summary>
        /// Add new GameObject with attached DropObject on specific place in hierarchy.
        /// </summary>
        /// <param name="parent">Parent for new GameObject.</param>
        /// <param name="prefab">Original object to copy.</param>
        /// <param name="name">Specific name for new Game object.</param>
        /// <returns>Created instance of DropObject.</returns>
        public DropObject AddDropObject(Transform parent, DropObject prefab = null, string name = "NewDropObject")
        {
            DropObject dropObject;
            if (prefab == null)
            {
                dropObject = new GameObject(name, typeof(RectTransform), typeof(DropObject), typeof(Image)).GetComponent<DropObject>();
            }
            else
            {
                dropObject = Instantiate(prefab, parent);
                dropObject.name = name;
            }

            dropObject.transform.SetParent(parent);
            CacheDropObject(dropObject);

            return dropObject;
        }

        /// <summary>
        /// Destroy GameObject and remove from cache specific instance of DragElement
        /// </summary>
        /// <param name="dragElement"></param>
        public void Destroy(DragElement dragElement)
        {
            if (!this.DragElementsCache.ContainsKey(dragElement.Id))
            {
                Object.Destroy(dragElement.gameObject);
                return;
            }

            Object.Destroy(this.DragElementsCache[dragElement.Id].gameObject);
            this.DragElementsCache.Remove(dragElement.Id);
        }

        /// <summary>
        /// Destroy GameObject and remove from cache specific instance of DropObject
        /// </summary>
        /// <param name="dropObject"></param>
        public void Destroy(DropObject dropObject)
        {
            if (!this.DropObjectsCache.ContainsKey(dropObject.Id))
            {
                Object.Destroy(dropObject.gameObject);
                return;
            }

            Object.Destroy(this.DropObjectsCache[dropObject.Id].gameObject);
            this.DropObjectsCache.Remove(dropObject.Id);
        }

        /// <summary>
        /// Cache all DragElements which are stored in <see cref="dragElements"/> on start.
        /// </summary>
        private void CacheDragElements()
        {
            for (int i = 0; i < this.dragElements.Length; i++)
            {
                CacheDragElement(this.dragElements[i]);
            }
        }

        /// <summary>
        /// Cache all DropObjects which are stored in <see cref="dropObjects"/> on start.
        /// </summary>
        private void CacheDropObjects()
        {
            for (int i = 0; i < this.dropObjects.Length; i++)
            {
                CacheDropObject(this.dropObjects[i]);
            }
        }

        /// <summary>
        /// Attach on specific DragElement all officially drag events.
        /// </summary>
        /// <param name="dragElement">Specific DragElement for preparing.</param>
        private void PrepareDragEvents(DragElement dragElement)
        {
            dragElement.OnBeginDragCallback = () => { LoadBeginDragEvents(dragElement); };

            dragElement.OnDragCallback = () => LoadDragEvents();

            dragElement.OnEndDragCallback = () => LoadEndDragEvents();

            void LoadBeginDragEvents(DragElement element)
            {
                this.SelectedDragElement = element;
                this.SelectedDragElement.TransformCache.SetParent(this.transform);

                if (this.HoveredDropObject != null)
                {
                    this.LastDropObject = this.HoveredDropObject;
                }

                this.onBeginDrag.Invoke();
            }

            void LoadDragEvents()
            {
                this.onDrag.Invoke();
            }

            void LoadEndDragEvents()
            {
                this.onEndDrag.Invoke();

                if (this.HoveredDropObject != null)
                {
                    if (this.HoveredDropObject.isEmpty)
                    {
                        this.SelectedDragElement.TransformCache.SetParent(this.HoveredDropObject.TransformCache);
                        this.SelectedDragElement.TransformCache.localPosition = Vector2.zero;
                        this.SelectedDragElement.SetLastParent();
                        this.HoveredDropObject.isEmpty = false;
                        LastDropObjectSetEmpty(true);
                    }
                    else
                    {
                        this.SelectedDragElement.ReturnToLastParent();
                        LastDropObjectSetEmpty(false);
                    }
                }
                else if (this.HoveredDropObject == null)
                {
                    this.onDropElementOutside.Invoke();
                    LastDropObjectSetEmpty(true);
                }


                this.LastSelectedDragElement = this.SelectedDragElement;
                this.SelectedDragElement = null;
            }
        }

        /// <summary>
        /// Attach on specific DropObject all officially drag events.
        /// </summary>
        /// <param name="dropObject">Specific DropObject for preparing.</param>
        private void PrepareDropEvents(DropObject dropObject)
        {
            dropObject.OnPointerEnterCallback = () => { LoadPointerEnterEvents(dropObject); };

            dropObject.OnPointerExitCallback = () => { LoadPointerExitEvents(); };

            void LoadPointerEnterEvents(DropObject оbject)
            {
                this.HoveredDropObject = оbject;
            }

            void LoadPointerExitEvents()
            {
                this.HoveredDropObject = null;
            }
        }

        /// <summary>
        /// Attach events and Cache specific DragElement in <see cref="DragElementsCache"/>
        /// </summary>
        /// <param name="dragElement"></param>
        public void CacheDragElement(DragElement dragElement)
        {
            PrepareDragEvents(dragElement);

            if (!this.DragElementsCache.ContainsKey(dragElement.Id))
            {
                this.DragElementsCache.Add(dragElement.Id, null);
            }

            this.DragElementsCache[dragElement.Id] = dragElement;
            this.DragElementsCache[dragElement.Id].IsCached = true;
        }

        /// <summary>
        /// Attach events and Cache specific DragElement in <see cref="DropObjectsCache"/>
        /// </summary>
        /// <param name="dropObject"></param>
        public void CacheDropObject(DropObject dropObject)
        {
            PrepareDropEvents(dropObject);

            Guid id = dropObject.Id;
            if (!this.DropObjectsCache.ContainsKey(id))
            {
                this.DropObjectsCache.Add(id, null);
            }

            this.DropObjectsCache[id] = dropObject;
            this.DropObjectsCache[id].IsCached = true;
        }

        /// <summary>
        /// Change <see cref="isEmpty"/> on <see cref="LastDropObject"/>.
        /// </summary>
        /// <param name="isEmpty">New Value</param>
        private void LastDropObjectSetEmpty(bool isEmpty)
        {
            if (this.LastDropObject != null)
            {
                this.LastDropObject.isEmpty = isEmpty;
            }
        }

        /// <summary>
        /// Add component when that component is missing from specific GameObject.
        /// </summary>
        /// <typeparam name="T">Type of new checked component.</typeparam>
        /// <param name="gameObject">GameObject which will check for missing component.</param>
        /// <returns></returns>
        public static T AddMissingComponent<T>(GameObject gameObject) where T : Component
        {
            T found = gameObject.GetComponent<T>();

            if (found == null)
            {
                return gameObject.AddComponent<T>();
            }

            return found;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            enableLogs = this.debugLogs;

            if (this.transform.root.GetComponentsInChildren<DragAndDrop>().Length > 1)
            {
                Log("More than one DragAndDrop instance has detected. You may have logical problems. Please centralize DragAndDrop logic on one script.", LogType.Warning, this.gameObject);
            }

            this.dragElements = this.transform.root.GetComponentsInChildren<DragElement>(true);
            this.dropObjects = this.transform.root.GetComponentsInChildren<DropObject>(true);
        }

        /// <summary>
        /// Centralized log message on unity Debug console.
        /// </summary>
        /// <param name="message">Specific message for print.</param>
        /// <param name="logType">Type of the message.</param>
        /// <param name="GameObjectToPoint"></param>
        public static void Log(string message, LogType logType, GameObject GameObjectToPoint = null)
        {
            if (!enableLogs)
                return;

            switch (logType)
            {
                case LogType.Info:
                    Debug.Log($"{DragAndDropString}{message}", GameObjectToPoint);
                    break;
                case LogType.Warning:
                    Debug.LogWarning($"{DragAndDropString}{message}", GameObjectToPoint);
                    break;
                case LogType.Error:
                    Debug.LogError($"{DragAndDropString}{message}", GameObjectToPoint);
                    break;
            }
        }

        /// <summary>
        /// Message log types.
        /// </summary>
        public enum LogType
        {
            Info,
            Warning,
            Error
        }
#endif
    }
}
