namespace Assets.Scripts.Core
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public static class DragAndDropEditor
    {
#if UNITY_EDITOR
        /// <summary>
        /// Create new instance of DragAndDrop in selected GameObject in hierarchy with one DragElement and one DropObject.
        /// </summary>
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

        /// <summary>
        /// Create new GameObject with attached DragElement in selected GameObject in hierarchy.
        /// </summary>
        [MenuItem("GameObject/DragAndDrop/CreateDragElement %DO", false)]
        [MenuItem("DragAndDrop/CreateDragElement %DE", false)]
        [MenuItem("CONTEXT/DragAndDrop/CreateDragElement %DE", false)]
        public static void AddDragElementOnSelected()
        {
            DragAndDrop dragAndDrop = Selection.activeGameObject.transform.root.GetComponent<DragAndDrop>();
            if (dragAndDrop == null)
            {
                DragAndDrop.Log("Don't have [DragAndDrop] Component in scene. Please assign and try again", DragAndDrop.LogType.Error);
                return;
            }

            DragElement dragElement = dragAndDrop.AddDragElement(Selection.activeGameObject.transform);
            Undo.RegisterCreatedObjectUndo(dragElement, "Create DragElement");
        }

        /// <summary>
        /// Create new GameObject with attached DropObject in selected GameObject in hierarchy.
        /// </summary>
        [MenuItem("GameObject/DragAndDrop/CreateDropObject %DO", false)]
        [MenuItem("DragAndDrop/CreateDropObject %DO", false)]
        [MenuItem("CONTEXT/DragAndDrop/CreateDropObject %DO", false)]
        public static void AddDropObjectOnSelected()
        {
            DragAndDrop dragAndDrop = Selection.activeGameObject.transform.root.GetComponent<DragAndDrop>();
            if (dragAndDrop == null)
            {
                DragAndDrop.Log("Don't have [DragAndDrop] Component in scene. Please assign and try again", DragAndDrop.LogType.Error);
                return;
            }

            DropObject dropObject = dragAndDrop.AddDropObject(Selection.activeGameObject.transform);
            Undo.RegisterCreatedObjectUndo(dropObject, "Create DropObject");
        }
#endif
    }
}