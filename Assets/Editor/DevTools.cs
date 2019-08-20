namespace Assets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

/*To create a hotkey you can use the following special characters: % (ctrl), # (shift), & (alt). 
If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use
"MyMenu/Do Something #&g". To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left.The keys supported 
like this are: LEFT, RIGHT, UP, DOWN, F1..F12, HOME, END, PGUP, PGDN.
*/

    public static class DevTools
    {
        public enum Direction
        {
            Up, Down, Left, Right
        }

        public enum AnchorType
        {
            Strech,
            Top,
            Middle,
            Bottom,
            Left,
            Center,
            Right,
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        public static float moveFactor = 0.5f;
        private static int waitCounter = 5;
        private static Direction selectedDirection;

        #region CREATE

        [MenuItem("DevTools/Create/SpriteRenderer Child %`", false)]
        public static void CreateSpriteRendererChild()
        {
            SpriteRenderer image = CreateObjectToSelected<SpriteRenderer>();
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/SpriteRenderer Sibling %&`", false)]
        public static void CreateSpriteRendererSibling()
        {
            SpriteRenderer image = CreateObjectToSelected<SpriteRenderer>(true);
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/Image Child %1", false)]
        public static void CreateChildImage()
        {
            Image image = CreateObjectToSelected<Image>();
            image.raycastTarget = false;
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/Image Sibling %F1", false)]
        public static void CreateSiblingImage()
        {
            Image image = CreateObjectToSelected<Image>(true);
            image.raycastTarget = false;
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/RawImage Child %2", false)]
        public static void CreateChildRawImage()
        {
            RawImage image = CreateObjectToSelected<RawImage>();
            image.raycastTarget = false;
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/RawImage Sibling %F2", false)]
        public static void CreateSiblingRawImage()
        {
            RawImage rawImage = CreateObjectToSelected<RawImage>(true);
            rawImage.raycastTarget = false;
            Selection.activeGameObject = rawImage.gameObject;
        }

        [MenuItem("DevTools/Create/Text Child %3", false)]
        public static void CreateChildText()
        {
            Text text = CreateObjectToSelected<Text>();
            text.raycastTarget = false;
            text.fontSize = 21;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = text.fontSize;
            text.rectTransform.sizeDelta = new Vector2(50, text.fontSize);
            text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Textures/Fonts/Open_Sans/OpenSans-Regular.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.supportRichText = false;
            Selection.activeGameObject = text.gameObject;
        }

        [MenuItem("DevTools/Create/Text Sibling %F3", false)]
        public static void CreateSiblingText()
        {
            Text text = CreateObjectToSelected<Text>(true);
            text.raycastTarget = false;
            text.fontSize = 21;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = text.fontSize;
            text.rectTransform.sizeDelta = new Vector2(100, text.fontSize);
            text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Textures/Fonts/Open_Sans/OpenSans-Regular.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.supportRichText = false;
            Selection.activeGameObject = text.gameObject;
        }

        [MenuItem("DevTools/Create/Button Child %4", false)]
        public static void CreateChildButton()
        {
            Image image = CreateObjectToSelected<Image>();
            image.gameObject.name = "Button";
            Button button = image.gameObject.AddMissingComponent<Button>();
            button.interactable = true;
            button.transition = Selectable.Transition.None;
            Navigation navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };

            button.navigation = navigation;

            image.raycastTarget = true;
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/Button Sibling %F4", false)]
        public static void CreateSiblingButton()
        {
            Image image = CreateObjectToSelected<Image>(true);
            image.gameObject.name = "Button";
            Button button = image.gameObject.AddMissingComponent<Button>();
            button.interactable = true;
            button.transition = Selectable.Transition.None;
            Navigation navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };

            button.navigation = navigation;

            image.raycastTarget = true;
            Selection.activeGameObject = image.gameObject;
        }

        [MenuItem("DevTools/Create/Scroll Masked Rect Child %5", false)]
        public static void CreateChildScrollRect()
        {
            ScrollRect view = CreateObjectToSelected<ScrollRect>();
            CreateViewItems(view);
        }

        [MenuItem("DevTools/Create/Scroll Masked Rect Sibling %F5", false)]
        public static void CreateSiblingScrollRect()
        {
            ScrollRect view = CreateObjectToSelected<ScrollRect>(true);
            CreateViewItems(view);
        }

        [MenuItem("DevTools/Create/Grid Child %6", false)]
        public static void CreateChildGrid()
        {
            VerticalLayoutGroup grid = CreateObjectToSelected<VerticalLayoutGroup>();
            grid.gameObject.AddMissingComponent<ContentSizeFitter>();
            Selection.activeGameObject = grid.gameObject;
            LayoutElement child = CreateObjectToSelected<LayoutElement>();
            Selection.activeGameObject = child.gameObject;
        }

        [MenuItem("DevTools/Create/Grid Sibling %F6", false)]
        public static void CreateSiblingGrid()
        {
            ScrollRect grid = CreateObjectToSelected<ScrollRect>(true);
            grid.gameObject.AddMissingComponent<ContentSizeFitter>();
            Selection.activeGameObject = grid.gameObject;
            LayoutElement child = CreateObjectToSelected<LayoutElement>();
            Selection.activeGameObject = child.gameObject;
        }

        private static T CreateObjectToSelected<T>(bool wantSibling = false) where T : Component
        {
            GameObject child = new GameObject(typeof(T).Name);
            Undo.RegisterCreatedObjectUndo(child, "Create GameObject");

            T component = child.AddMissingComponent<T>();
            if (Selection.activeGameObject != null)
            {
                child.transform.parent = wantSibling ? Selection.activeGameObject.transform.parent : Selection.activeGameObject.transform;
            }

            RectTransform rect = child.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localPosition = Vector3.zero;
                rect.localRotation = Quaternion.Euler(Vector3.zero);
                rect.localScale = Vector3.one;
            }

            return component;
        }

        #endregion

        #region ANCHORS

        [MenuItem("DevTools/Anchors/Strech %[]")]
        private static void FixAnchorsStrech()
        {
            FixAnchors(AnchorType.Strech);
            //GameObject o = Selection.activeGameObject;
            //Undo.RecordObject(o, "Anchor " + o);

            //if (o != null && o.GetComponent<RectTransform>() != null)
            //{
            //    RectTransform r = o.GetComponent<RectTransform>();
            //    RectTransform p = o.transform.parent.GetComponent<RectTransform>();

            //    Vector2 offsetMin = r.offsetMin;
            //    Vector2 offsetMax = r.offsetMax;
            //    Vector2 _anchorMin = r.anchorMin;
            //    Vector2 _anchorMax = r.anchorMax;

            //    float parent_width = p.rect.width;
            //    float parent_height = p.rect.height;

            //    Vector2 anchorMin = new Vector2(_anchorMin.x + offsetMin.x / parent_width,
            //                                _anchorMin.y + (offsetMin.y / parent_height));
            //    Vector2 anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
            //                                _anchorMax.y + (offsetMax.y / parent_height));

            //    r.anchorMin = anchorMin;
            //    r.anchorMax = anchorMax;

            //    r.offsetMin = Vector2.zero;
            //    r.offsetMax = Vector2.one;
            //    r.pivot = new Vector2(0.5f, 0.5f);
            //}
        }

        [MenuItem("DevTools/Anchors/Top")]
        private static void FixAnchorsTop()
        {
            FixAnchors(AnchorType.Top);
        }

        [MenuItem("DevTools/Anchors//Middle")]
        private static void FixAnchorsMiddle()
        {
            FixAnchors(AnchorType.Middle);
        }

        [MenuItem("DevTools/Anchors/Bottom")]
        private static void FixAnchorsBottom()
        {
            FixAnchors(AnchorType.Bottom);
        }

        [MenuItem("DevTools/Anchors/Left")]
        private static void FixAnchorsLeft()
        {
            FixAnchors(AnchorType.Left);
        }

        [MenuItem("DevTools/Anchors/Center")]
        private static void FixAnchorsCenter()
        {
            FixAnchors(AnchorType.Center);
        }

        [MenuItem("DevTools/Anchors/Right")]
        private static void FixAnchorsRight()
        {
            FixAnchors(AnchorType.Right);
        }

        [MenuItem("DevTools/Anchors/Top Left")]
        private static void FixAnchorsTopLeft()
        {
            FixAnchors(AnchorType.TopLeft);
        }

        [MenuItem("DevTools/Anchors/Top Center")]
        private static void FixAnchorsTopCenter()
        {
            FixAnchors(AnchorType.TopCenter);
        }

        [MenuItem("DevTools/Anchors/Top Right")]
        private static void FixAnchorsTopRight()
        {
            FixAnchors(AnchorType.TopRight);
        }

        [MenuItem("DevTools/Anchors/Middle Left")]
        private static void FixAnchorsMiddleLeft()
        {
            FixAnchors(AnchorType.MiddleLeft);
        }

        [MenuItem("DevTools/Anchors/Middle Center")]
        private static void FixAnchorsMiddleCenter()
        {
            FixAnchors(AnchorType.MiddleCenter);
        }

        [MenuItem("DevTools/Anchors/Middle Right")]
        private static void FixAnchorsMiddleRight()
        {
            FixAnchors(AnchorType.MiddleRight);
        }

        [MenuItem("DevTools/Anchors/Bottom Left")]
        private static void FixAnchorsBottomLeft()
        {
            FixAnchors(AnchorType.BottomLeft);
        }

        [MenuItem("DevTools/Anchors/Bottom Center")]
        private static void FixAnchorsBottomCenter()
        {
            FixAnchors(AnchorType.BottomCenter);
        }

        [MenuItem("DevTools/Anchors/Bottom Right")]
        private static void FixAnchorsBottomRight()
        {
            FixAnchors(AnchorType.BottomRight);
        }

        [MenuItem("DevTools/Anchors/Offset")]
        public static void CheckOffSet()
        {
            GameObject currGo = Selection.activeGameObject;
            Vector2 revVector = new Vector2(1, 1);

            revVector = Camera.main.ViewportToScreenPoint(revVector);
            currGo.transform.InverseTransformPoint(revVector);
            Debug.Log(currGo.transform.position);
        }

        private static void FixAnchors(AnchorType type, GameObject[] gameObjs = null)
        {
            if (gameObjs == null || gameObjs.Length == 0)
            {
                gameObjs = Selection.gameObjects;
            }

            foreach (GameObject go in gameObjs)
            {
                RectTransform rectTransform = go.GetComponent<RectTransform>();

                if (rectTransform == null)
                {
                    continue;
                }

                Vector3[] corners = new Vector3[4];
                Vector3[] parentCorners = new Vector3[4];

                RectTransform parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();

                Vector3 originalScale = rectTransform.localScale;
                rectTransform.localScale = Vector3.one;

                Quaternion originalRotation = rectTransform.rotation;
                rectTransform.rotation = Quaternion.identity;

                if (parentRectTransform == null)
                {
                    Debug.Log(string.Format("Selected element {0} has no RectTransform Component.GameObject is skipped", go.name));
                    continue;
                }

                bool hasAspectRatio = false;
                if (go.GetComponent<AspectRatioFitter>() != null && go.GetComponent<AspectRatioFitter>().enabled == true)
                {
                    hasAspectRatio = true;
                    go.GetComponent<AspectRatioFitter>().enabled = false;
                }

                Canvas canvas = rectTransform.GetComponentInParent<Canvas>();

                rectTransform.GetWorldCorners(corners);
                parentRectTransform.GetWorldCorners(parentCorners);
                //Debug.Log("rectTransform.rect.width : " + rectTransform.rect.width);
                //Debug.Log("rectTransform.rect.height : " + rectTransform.rect.height);
                float rectUIWidth = canvas.transform.localScale.x * rectTransform.rect.width;// * rectTransform.lossyScale.x; 
                float rectUIHeight = canvas.transform.localScale.y * rectTransform.rect.height;// * rectTransform.lossyScale.y; 
                float parentUIWidth = parentRectTransform.rect.width * canvas.transform.localScale.x;// * parentRectTransform.lossyScale.x; 
                float parentUIHeight = parentRectTransform.rect.height * canvas.transform.localScale.y;// * parentRectTransform.lossyScale.y; 

                Vector2 parentZeroCorner = parentCorners[0];
                float parent_x = parentZeroCorner.x + ((parentCorners[3].x - parentZeroCorner.x) / 2);
                float parent_y = parentZeroCorner.y + ((parentCorners[1].y - parentZeroCorner.y) / 2);
                float parentXMin = parent_x - (parentUIWidth / 2);
                float parentYMin = parent_y - (parentUIHeight / 2);

                float minX = corners[0].x;
                float maxY = corners[3].y;

                Vector2 anchorMinPoint = new Vector2(minX - parentXMin, maxY - parentYMin);
                Vector2 anchorMaxPoint = Vector2.zero;
                Vector2 offsetMin = Vector2.zero;
                Vector2 offsetMax = Vector2.zero;

                switch (type)
                {
                    case AnchorType.Strech:
                        anchorMaxPoint.x = anchorMinPoint.x + rectUIWidth;
                        anchorMaxPoint.y = anchorMinPoint.y + rectUIHeight;
                        break;

                    case AnchorType.Top:
                        anchorMinPoint.y += rectUIHeight;
                        anchorMaxPoint.x = anchorMinPoint.x + rectUIWidth;
                        anchorMaxPoint.y = anchorMinPoint.y;
                        offsetMax = Vector2.zero;
                        offsetMin = new Vector2(0f, -rectTransform.rect.height);
                        break;

                    case AnchorType.Middle:
                        anchorMinPoint.y += rectUIHeight / 2;
                        anchorMaxPoint.x = anchorMinPoint.x + rectUIWidth;
                        anchorMaxPoint.y = anchorMinPoint.y;
                        offsetMax = new Vector2(0f, +rectTransform.rect.height / 2);
                        offsetMin = new Vector2(0f, -rectTransform.rect.height / 2);
                        break;

                    case AnchorType.Bottom:
                        anchorMaxPoint.x = anchorMinPoint.x + rectUIWidth;
                        anchorMaxPoint.y = anchorMinPoint.y;
                        offsetMax = new Vector2(0f, rectTransform.rect.height);
                        offsetMin = Vector2.zero;
                        break;

                    case AnchorType.Left:
                        anchorMaxPoint.x = anchorMinPoint.x;
                        anchorMaxPoint.y = anchorMinPoint.y + rectUIHeight;
                        offsetMax = new Vector2(rectTransform.rect.width, 0f);
                        offsetMin = Vector2.zero;
                        break;

                    case AnchorType.Center:
                        anchorMinPoint.x += rectUIWidth / 2;
                        anchorMaxPoint.x = anchorMinPoint.x;
                        anchorMaxPoint.y = anchorMinPoint.y + rectUIHeight;
                        offsetMax = new Vector2(rectTransform.rect.width / 2, 0f);
                        offsetMin = new Vector2(-rectTransform.rect.width / 2, 0f);
                        break;

                    case AnchorType.Right:
                        anchorMinPoint.x += rectUIWidth;
                        anchorMaxPoint.x = anchorMinPoint.x;
                        anchorMaxPoint.y = anchorMinPoint.y + rectUIHeight;
                        offsetMax = Vector2.zero;
                        offsetMin = new Vector2(-rectTransform.rect.width, 0f);
                        break;

                    case AnchorType.TopLeft:
                        anchorMinPoint.y += rectUIHeight;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(rectTransform.rect.width, 0f);
                        offsetMin = new Vector2(0f, -rectTransform.rect.height);
                        break;

                    case AnchorType.TopCenter:
                        anchorMinPoint.x += rectUIWidth / 2;
                        anchorMinPoint.y += rectUIHeight;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(rectTransform.rect.width / 2, 0f);
                        offsetMin = new Vector2(-rectTransform.rect.width / 2, -rectTransform.rect.height);
                        break;

                    case AnchorType.TopRight:
                        anchorMinPoint.x += rectUIWidth;
                        anchorMinPoint.y += rectUIHeight;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = Vector2.zero;
                        offsetMin = new Vector2(-rectTransform.rect.width, -rectTransform.rect.height);
                        break;

                    case AnchorType.MiddleLeft:
                        anchorMinPoint.y += rectUIHeight / 2;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(rectTransform.rect.width, rectTransform.rect.height / 2);
                        offsetMin = new Vector2(0f, -rectTransform.rect.height / 2);
                        break;

                    case AnchorType.MiddleCenter:
                        anchorMinPoint.x += rectUIWidth / 2;
                        anchorMinPoint.y += rectUIHeight / 2;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height / 2);
                        offsetMin = new Vector2(-rectTransform.rect.width / 2, -rectTransform.rect.height / 2);
                        break;

                    case AnchorType.MiddleRight:
                        anchorMinPoint.x += rectUIWidth;
                        anchorMinPoint.y += rectUIHeight / 2;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(0f, rectTransform.rect.height / 2);
                        offsetMin = new Vector2(-rectTransform.rect.width, -rectTransform.rect.height / 2);
                        break;

                    case AnchorType.BottomLeft:
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
                        offsetMin = Vector2.zero;
                        break;

                    case AnchorType.BottomCenter:
                        anchorMinPoint.x += rectUIWidth / 2;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height);
                        offsetMin = new Vector2(-rectTransform.rect.width / 2, 0f);
                        break;

                    case AnchorType.BottomRight:
                        anchorMinPoint.x += rectUIWidth;
                        anchorMaxPoint = anchorMinPoint;
                        offsetMax = new Vector2(0f, rectTransform.rect.height);
                        offsetMin = new Vector2(-rectTransform.rect.width, 0f);
                        break;

                    default:
                        break;
                }

                //Convert to relative anchor coordinates
                anchorMinPoint.x /= parentUIWidth;
                anchorMinPoint.y /= parentUIHeight;

                anchorMaxPoint.x /= parentUIWidth;
                anchorMaxPoint.y /= parentUIHeight;


                //apply new anchors
                rectTransform.anchorMin = anchorMinPoint;
                rectTransform.anchorMax = anchorMaxPoint;
                rectTransform.offsetMax = offsetMax;
                rectTransform.offsetMin = offsetMin;

                if (hasAspectRatio)
                {
                    go.GetComponent<AspectRatioFitter>().enabled = true;
                }

                rectTransform.rotation = originalRotation;
                rectTransform.localScale = originalScale;


                //Debug.Log("after rectTransform.rect.width : " + rectTransform.rect.width);
                //Debug.Log("after rectTransform.rect.height : " + rectTransform.rect.height);
            }
        }

        #endregion

        #region OTHER

        [MenuItem("DevTools/Other/Atributes")]
        public static void LogAtributes()
        {
            Debug.Log(
                "Editor:\n [HideInInspector]\n  [ExecuteInEditMode]\n  [ContextMenu()]\n  [CanEditMultipleObjects]\n  " +
                "[MenuItem()]\n  [Range()]\n  [Tooltip()]\n  [Header]\n  [Space()]\n  [RequireComponent()]\n  [Serializable]\n  [SerializeField]\n  [TextArea()]"
            );
        }

        [MenuItem("DevTools/Other/Update Unity 3D")]
        public static void UpdateUnity3D()
        {
            Application.OpenURL("https://unity3d.com/get-unity/update");
        }

        #endregion

        #region MOVE

        [MenuItem("DevTools/Transform/Move/↑ %UP")]
        public static void MoveTransformUp()
        {
            MoveSelected(Direction.Up);
        }

        [MenuItem("DevTools/Transform/Move/↓ %DOWN")]
        public static void MoveTransformDown()
        {
            MoveSelected(Direction.Down);
        }

        [MenuItem("DevTools/Transform/Move/←| %LEFT")]
        public static void MoveTransformLeft()
        {
            MoveSelected(Direction.Left);
        }

        [MenuItem("DevTools/Transform/Move/|→ %RIGHT")]
        public static void MoveTransformRight()
        {
            MoveSelected(Direction.Right);
        }

        public static void MoveSelected(Direction direction)
        {
            Transform transform = Selection.activeGameObject.transform;
            Undo.RecordObject(transform, "Move " + direction);
            AdjustMoveSpeedOnHold(direction);
            switch (direction)
            {
                case Direction.Up:
                    transform.localPosition = new Vector3(
                        transform.localPosition.x,
                        transform.localPosition.y + moveFactor,
                        transform.localPosition.z);
                    break;
                case Direction.Down:
                    transform.localPosition = new Vector3(
                        transform.localPosition.x,
                        transform.localPosition.y - moveFactor,
                        transform.localPosition.z);
                    break;
                case Direction.Left:
                    transform.localPosition = new Vector3(
                        transform.localPosition.x - moveFactor,
                        transform.localPosition.y,
                        transform.localPosition.z);
                    break;
                case Direction.Right:
                    transform.localPosition = new Vector3(
                        transform.localPosition.x + moveFactor,
                        transform.localPosition.y,
                        transform.localPosition.z);
                    break;
            }
        }

        private static void AdjustMoveSpeedOnHold(Direction direction)
        {
            if (selectedDirection == direction)
            {
                waitCounter++;
                if (waitCounter == 30)
                {
                    moveFactor += 1;
                }
            }
            else
            {
                moveFactor = 0.5f;
                waitCounter = 0;
                selectedDirection = direction;
            }
        }

        #endregion

        [MenuItem("CONTEXT/Graphic/Copy color %#C", false)]
        public static void CopyColor()
        {
            GameObject activeGameObject = Selection.activeGameObject;
            string colorForCopy = string.Empty;

            if (activeGameObject.GetComponent<Graphic>() != null)
            {
                colorForCopy = ((Color32)activeGameObject.GetComponent<Graphic>().color).ToString();
            }
            else if (activeGameObject.GetComponent<Renderer>() != null)
            {
                colorForCopy = ((Color32)activeGameObject.GetComponent<Graphic>().color).ToString();
            }

            EditorGUIUtility.systemCopyBuffer = colorForCopy.Replace("RGBA(", "").Replace(")", "").Trim();
        }

        [MenuItem("DevTools/Graphic/Disable Raycast Without Button")]
        public static void DisableRaycast()
        {
            Graphic[] allObjects = Resources.FindObjectsOfTypeAll<Graphic>();
            Undo.RecordObjects(allObjects, "Disable Raycast Without Button");
            for (int i = 0; i < allObjects.Length; i++)
            {
                Graphic graphic = allObjects[i];
                Button button = graphic.GetComponent<Button>();
                ScrollRect rect = graphic.GetComponent<ScrollRect>();
                CanvasGroup cg = graphic.GetComponent<CanvasGroup>();

                graphic.raycastTarget = false;

                if (button != null)
                {
                    graphic.raycastTarget = true;
                    continue;
                }

                if (rect != null)
                {
                    graphic.raycastTarget = true;
                    continue;
                }

                if (cg != null)
                {
                    graphic.raycastTarget = true;
                }
            }
        }

        [MenuItem("CONTEXT/Graphic/Аdd Button to existing Image &B")]
        public static void AddButtonAndImage()
        {
            GameObject[] objects = TakeSelectionGameObjects(Selection.gameObjects);
            Undo.RecordObjects(objects, "Add Button to existing Image");

            for (int i = 0; i < objects.Length; i++)
            {
                Image image = Utilities.AddMissingComponent<Image>(objects[i]);
                Button button = Utilities.AddMissingComponent<Button>(objects[i]);

                button.interactable = true;
                button.transition = Selectable.Transition.None;
                Navigation navigation = new Navigation
                {
                    mode = Navigation.Mode.None
                };

                button.navigation = navigation;

                image.raycastTarget = true;
            }
        }

        [MenuItem("DevTools/Graphic/Find texture in resourses %F")]
        public static void FindTexture()
        {
            if (Selection.activeGameObject.GetComponent<Graphic>() != null)
            {
                Graphic texture = Selection.activeGameObject.GetComponent<Graphic>();
                EditorGUIUtility.PingObject(texture.mainTexture);
            }
        }

        [MenuItem("DevTools/Graphic/Remove canvas renderer when is not needed")]
        public static void RemoveCanvasRendererWhenIsNotNeeded()
        {
            CanvasRenderer[] allObjects = Resources.FindObjectsOfTypeAll<CanvasRenderer>();

            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].GetComponent<Graphic>() == null)
                {
                    GameObject.DestroyImmediate(allObjects[i]);
                }
            }
        }

        [MenuItem("DevTools/Redo #%Z")]
        public static void RedoVS()
        {
            Undo.PerformRedo();
        }

        [MenuItem("DevTools/UI/Move Sibling Finish %&UP")]
        public static void MoveSiblingUp()
        {
            Transform transform = Selection.activeGameObject.transform;
            transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        }

        [MenuItem("DevTools/UI/Move Sibling Start %&DOWN")]
        public static void MoveSiblingDown()
        {
            Transform transform = Selection.activeGameObject.transform;
            transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        }

        [MenuItem("DevTools/UI/Move Sibling Out %&LEFT")]
        public static void MoveSiblingOut()
        {
            Transform transform = Selection.activeGameObject.transform;
            int parentLocation = transform.parent.GetSiblingIndex();
            transform.parent = transform.parent.parent;
            transform.SetSiblingIndex(parentLocation);
        }

        [MenuItem("DevTools/UI/Move Sibling In %&RIGHT")]
        public static void MoveSiblingIn()
        {
            Transform transform = Selection.activeGameObject.transform;
            bool reachedTarget = false;

            if (transform.parent == null)
            {
                foreach (GameObject go in SceneRoots())
                {
                    if (reachedTarget)
                    {
                        transform.transform.SetParent(go.transform);
                        SetExpandedRecursive(go, true);
                        for (int i = 0; i < go.transform.childCount; i++)
                        {
                            SetExpandedRecursive(go.transform.GetChild(i).gameObject, false);
                        }
                        Selection.activeGameObject = transform.gameObject;
                        transform.SetSiblingIndex(0);

                        break;
                    }
                    if (go == transform.gameObject)
                    {
                        reachedTarget = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    if (reachedTarget)
                    {
                        GameObject go = transform.parent.GetChild(i).gameObject;
                        transform.transform.SetParent(go.transform);
                        SetExpandedRecursive(go, true);
                        for (int j = 0; j < go.transform.childCount; j++)
                        {
                            SetExpandedRecursive(go.transform.GetChild(j).gameObject, false);
                        }
                        Selection.activeGameObject = transform.gameObject;
                        transform.SetSiblingIndex(0);
                        break;
                    }
                    if (transform == transform.parent.GetChild(i))
                    {
                        reachedTarget = true;
                    }
                }
            }
        }

        [MenuItem("DevTools/Transform/Reset all transforms scale bug")]
        public static void ResetScaleBug()
        {
            Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();
            Undo.RecordObjects(allObjects, "Reset all transforms scale bug");

            float min = 0.5f;
            float max = 1.1f;

            for (int i = 0; i < allObjects.Length; i++)
            {
                if (min < allObjects[i].localScale.x && allObjects[i].localScale.x < max ||
                    min < allObjects[i].localScale.y && allObjects[i].localScale.y < max ||
                    min < allObjects[i].localScale.z && allObjects[i].localScale.z < max)
                {
                    allObjects[i].localScale = Vector3.one;
                }
            }
        }

        [MenuItem("CONTEXT/Transform/Log Transform Props"),
         MenuItem("GameObject/Log Transform Props", false, 20)]
        public static void LogTransformInformation()
        {
            Transform[] transforms = TakeSelectionTransforms(Selection.transforms);

            foreach (Transform transform in transforms)
            {
                string message = string.Format("NAME:\"{0}\"" +
                                               "\nPOS: {1,-10:0.0####}, {2,-10:0.0####}, {3,-10:0.0####}" +
                                               "\nROT: {4,-10:0.0####}, {5,-10:0.0####}, {6,-10:0.0####}" +
                                               "\nSCA: {7,-10:0.0####}, {8,-10:0.0####}, {9,-10:0.0####}",
                    transform.name,
                    transform.localPosition.x, transform.localPosition.y, transform.localPosition.z,
                    transform.localRotation.x, transform.localRotation.y, transform.localRotation.z,
                    transform.localScale.x, transform.localScale.y, transform.localScale.z);

                RectTransform rectTransform = transform.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    message += string.Format("\n Width: {0} Height {1}", rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                }

                Debug.Log(message);
                Debug.DrawLine(Vector3.left, Vector3.right);
            }
        }

        //[MenuItem("DevTools/ScrollFade/RemoveAll SoftMask Scripts")]
        //public static void RemoveAllSoftMask()
        //{
        //    SoftMaskScript[] softMaskScripts = Selection.activeTransform.GetComponentsInChildren<SoftMaskScript>(true);
        //    foreach (SoftMaskScript maskScript in softMaskScripts)
        //    {
        //        GameObject.DestroyImmediate(maskScript);
        //    }

        //    Debug.Log("Soft Masks removed : " + softMaskScripts.Length);

        //    Image[] softMaksImages = Selection.activeTransform.GetComponentsInChildren<Image>(true);
        //    foreach (Image img in softMaksImages)
        //    {
        //        img.material = null;
        //    }

        //    Debug.Log("Images materials removed : " + softMaksImages.Length);

        //    RawImage[] rawImages = Selection.activeTransform.GetComponentsInChildren<RawImage>(true);
        //    foreach (RawImage img in rawImages)
        //    {
        //        img.material = null;
        //    }

        //    Debug.Log("RawImage materials removed : " + rawImages.Length);

        //}

        //[MenuItem("DevTools/ScrollFade/Refresh Soft Mask")]
        //public static void RefreshSoftMask()
        //{
        //    SoftMaskScript softMaskScript = Selection.activeTransform.GetComponent<SoftMaskScript>();
        //    if (softMaskScript != null)
        //    {
        //        softMaskScript.Refresh();
        //        Debug.Log("Refreshed Soft Mask");
        //    }

        //    EditorUtility.SetDirty(softMaskScript.gameObject);
        //}

        //[MenuItem("DevTools/ScrollFade/Refresh ALL Soft Masks")]
        //public static void RefreshALLSoftMask()
        //{
        //    SoftMaskScript[] softMaskScripts = GameObject.FindObjectsOfType<SoftMaskScript>();
        //    foreach (SoftMaskScript maskScript in softMaskScripts)
        //    {
        //        maskScript.Refresh();
        //    }
        //}

        [MenuItem("Assets/Log Path", false, 50)]
        [MenuItem("GameObject/Log Path", false, 20)]
        public static void LogPathInHierarchy()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (string.IsNullOrEmpty(path))
            {
                Transform trans = Selection.activeTransform;
                path = trans.name;
                while (trans.parent != null)
                {
                    trans = trans.parent;
                    path = trans.name + "/" + path;
                }
            }

            Debug.Log(path);
        }

        [MenuItem("Assets/Find texture in Hierarchy %#R", false, 49)]
        public static void FindTextureInHierarchy()
        {
            Graphic[] allObjects = Resources.FindObjectsOfTypeAll<Graphic>();
            if (allObjects.Length == 0)
            {
                Debug.Log("Dont have objects with Image components");
                return;
            }

            //Logs in console Clear().
            Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            Type type = assembly.GetType("UnityEditorInternal.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);

            bool dontHaveReferences = true;
            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].mainTexture == null)
                {
                    continue;
                }

                if (allObjects[i].mainTexture == Selection.activeObject)
                {
                    dontHaveReferences = false;
                    Debug.Log(allObjects[i].name, allObjects[i].gameObject);
                }
            }

            if (dontHaveReferences)
            {
                Debug.Log("Texture don't have references");
            }
        }

        [MenuItem("DevTools/Buttons/Remove navigations all buttons")]
        public static void RemoveNavigationFromButton()
        {
            Selectable[] allObjects = Resources.FindObjectsOfTypeAll<Selectable>(); 
            Undo.RecordObjects(allObjects, "Remove navigations all buttons");

            for (int i = 0; i < allObjects.Length; i++)
            {
                allObjects[i].navigation = default(Navigation);
            }
        }

        [MenuItem("DevTools/Buttons/Remove default transition")]
        public static void RemoveDefaultTransitions()
        {
            Selectable[] allObjects = Resources.FindObjectsOfTypeAll<Selectable>();
            Undo.RecordObjects(allObjects, "Remove default transition");

            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].transition == Selectable.Transition.ColorTint)
                {
                    if (allObjects[i].colors.normalColor == Color.white &&
                        allObjects[i].colors.highlightedColor == new Color32(245, 245, 245, 255) &&
                        allObjects[i].colors.pressedColor == new Color32(200, 200, 200, 255) &&
                        allObjects[i].colors.disabledColor == new Color32(200, 200, 200, 128))
                    {
                        allObjects[i].transition = Selectable.Transition.None;
                    }
                }
            }
        }

        [MenuItem("DevTools/Text/Remove Rich Text")]
        public static void RemoveRichText()
        {
            Text[] allObjects = Resources.FindObjectsOfTypeAll<Text>();
            Undo.RecordObjects(allObjects, "Remove Rich Text");

            for (int i = 0; i < allObjects.Length; i++)
            {
                allObjects[i].supportRichText = false;
            }
        }

        [MenuItem("DevTools/Text/Replace Bold Text with Regular")]
        public static void ReplaceBoldTextWithRegular()
        {
            Text[] allObjects = Resources.FindObjectsOfTypeAll<Text>();
            Undo.RecordObjects(allObjects, "Replace Bold Text with Regular");

            for (int i = 0; i < allObjects.Length; i++)
            {
                Text text = allObjects[i];
                if (text.font.name == "OpenSans-Bold")
                {
                    Font loadAssetAtPath = AssetDatabase.LoadAssetAtPath<Font>("Assets/Textures/Fonts/OpenSans-Regular.ttf");
                    text.font = loadAssetAtPath;
                    text.fontStyle = FontStyle.Bold;
                }
            }
        }

        [MenuItem("DevTools/Text/Replace Italic Text with Regular")]
        public static void ReplaceItalicTextWithRegular()
        {
            Text[] allObjects = Resources.FindObjectsOfTypeAll<Text>();
            Undo.RecordObjects(allObjects, "Replace Italic Text with Regular");

            for (int i = 0; i < allObjects.Length; i++)
            {
                Text text = allObjects[i];
                if (text.font.name == "OpenSans-Italic")
                {
                    Font loadAssetAtPath = AssetDatabase.LoadAssetAtPath<Font>("Assets/Textures/Fonts/OpenSans-Regular.ttf");
                    text.font = loadAssetAtPath;
                    text.fontStyle = FontStyle.Italic;
                }
            }
        }


        [MenuItem("DevTools/ScrollRect/Set default deceleration rate")]
        public static void SetDefaultDeselerationRate()
        {
            ScrollRect[] allObjects = Resources.FindObjectsOfTypeAll<ScrollRect>();
            Undo.RecordObjects(allObjects, "Set default deceleration rate");

            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].inertia)
                {
                    if (allObjects[i].horizontal || allObjects[i].vertical)
                    {
                        allObjects[i].decelerationRate = 0.0008f;
                    }
                }

                allObjects[i].scrollSensitivity = 30;
            }
        }

        private static GameObject[] TakeSelectionGameObjects(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                gameObjects = Selection.gameObjects;
            }

            return gameObjects;
        }

        private static Transform[] TakeSelectionTransforms(Transform[] transforms)
        {
            if (transforms == null || transforms.Length == 0)
            {
                transforms = Selection.transforms;
            }

            Undo.RecordObjects(transforms, "Disable Raycast to All");
            return transforms;
        }

        private static IEnumerable<GameObject> SceneRoots()
        {
            HierarchyProperty prop = new HierarchyProperty(HierarchyType.GameObjects);
            int[] expanded = new int[0];
            while (prop.Next(expanded))
            {
                yield return prop.pptrValue as GameObject;
            }
        }

        private static void SetExpandedRecursive(GameObject go, bool expand)
        {
            Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");

            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            EditorWindow window = EditorWindow.focusedWindow;

            methodInfo.Invoke(window, new object[] { go.GetInstanceID(), expand });
        }

        private static void CreateViewItems(ScrollRect view)
        {
            Image img = view.gameObject.AddMissingComponent<Image>();
            GameObject gridObject = new GameObject("Grid", typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            ContentSizeFitter fitter = gridObject.GetComponent<ContentSizeFitter>();
            GameObject gridChild = new GameObject("Button", typeof(Image), typeof(Button));
            Button button = gridChild.GetComponent<Button>();

            gridObject.transform.parent = view.transform;
            button.transform.parent = gridObject.transform;

            fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            img.color = new Color32(255, 255, 255, 0);

            view.gameObject.AddMissingComponent<RectMask2D>();
            view.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 768f);
            view.content = gridObject.GetComponent<RectTransform>();
            view.vertical = true;
            view.horizontal = false;
            view.movementType = ScrollRect.MovementType.Elastic;
            view.elasticity = 0.09f;
            view.inertia = true;
            view.decelerationRate = 0.003f;

            grid.GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);
            grid.cellSize = new Vector2(200f, 120f);
            grid.spacing = new Vector2(0f, 2f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 1;

            button.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 120f);
            button.interactable = true;
            button.transition = Selectable.Transition.None;
            Navigation navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };

            button.navigation = navigation;

            button.GetComponent<Image>().raycastTarget = true;
            button.GetComponent<Image>().color = new Color32(255, 255, 255, 0);


            Selection.activeGameObject = button.gameObject;
        }

        #region VALIDATIONS
        [
            MenuItem("DevTools/UI/Move Sibling Finish %&UP", true),
            MenuItem("DevTools/UI/Move Sibling Start %&DOWN", true),
            MenuItem("DevTools/UI/Move Sibling Out %&LEFT", true),
            MenuItem("DevTools/UI/Move Sibling In %&RIGHT", true),
            MenuItem("DevTools/Transform/Move/↑ %UP", true),
            MenuItem("DevTools/Transform/Move/↓ %DOWN", true),
            MenuItem("DevTools/Transform/Move/←| %LEFT", true),
            MenuItem("DevTools/Transform/Move/|→ %RIGHT", true),
            MenuItem("DevTools/Anchors/Strech %[]", true),
            MenuItem("DevTools/Anchors/Top", true),
            MenuItem("DevTools/Anchors//Middle", true),
            MenuItem("DevTools/Anchors/Bottom", true),
            MenuItem("DevTools/Anchors/Left", true),
            MenuItem("DevTools/Anchors/Center", true),
            MenuItem("DevTools/Anchors/Right", true),
            MenuItem("DevTools/Anchors/Top Left", true),
            MenuItem("DevTools/Anchors/Top Center", true),
            MenuItem("DevTools/Anchors/Top Right", true),
            MenuItem("DevTools/Anchors/Middle Left", true),
            MenuItem("DevTools/Anchors/Middle Center", true),
            MenuItem("DevTools/Anchors/Middle Right", true),
            MenuItem("DevTools/Anchors/Bottom Left", true),
            MenuItem("DevTools/Anchors/Bottom Center", true),
            MenuItem("DevTools/Anchors/Bottom Right", true),
            MenuItem("DevTools/Anchors/Offset", true)
        ]
        private static bool HaveActiveObject()
        {
            return Selection.activeGameObject != null;
        }
        #endregion
    }

    public class GameViewUtils
    {
        static object gameViewSizesInstance;
        static MethodInfo getGroup;
        private static int screenIndex = 16;
        private static int gameViewProfilesCount;

        static GameViewUtils()
        {
            // gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            getGroup = sizesType.GetMethod("GetGroup");
            gameViewSizesInstance = instanceProp.GetValue(null, null);
        }

        private enum GameViewSizeType
        {
            AspectRatio, FixedResolution
        }

        private static void SetSize(int index)
        {
            var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            var SizeSelectionCallback = gvWndType.GetMethod("SizeSelectionCallback",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            SizeSelectionCallback.Invoke(gvWnd, new object[] { index, null });
        }

        static object GetGroup(GameViewSizeGroupType type)
        {
            return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
        }

        [MenuItem("DevTools/GameViewSize/Previous %&Q")]
        private static void SetPrevious()
        {
            GetViewListSize();
            if (screenIndex - 1 >= 16)
            {
                screenIndex -= 1;
            }
            else
            {
                screenIndex = gameViewProfilesCount - 1;
            }

            SetSize(screenIndex);
        }

        [MenuItem("DevTools/GameViewSize/Next  %&E")]
        private static void SetNext()
        {
            GetViewListSize();
            if (screenIndex + 1 < gameViewProfilesCount)
            {
                screenIndex += 1;
            }
            else
            {
                screenIndex = 16;
            }

            SetSize(screenIndex);
        }

        private static void GetViewListSize()
        {
            var group = GetGroup(GameViewSizeGroupType.Android);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            gameViewProfilesCount = (getDisplayTexts.Invoke(group, null) as string[]).Length;
        }
    }
}