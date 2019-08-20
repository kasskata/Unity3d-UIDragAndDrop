using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class Utilities
{
    private static Material grayScaleMaterial;
    public static Material GrayScaleMaterial
    {
        get
        {
            if (grayScaleMaterial == null)
            {
                grayScaleMaterial = Resources.Load<Material>("Materials/GrayscaleUI");
            }

            return grayScaleMaterial;
        }
    }

    private static Material hueMaterial;
    public static Material HueMaterial
    {
        get
        {
            if (hueMaterial == null)
            {
                hueMaterial = Resources.Load<Material>("Materials/HSBC");
            }

            return hueMaterial;
        }
    }

    public static string TimeFormat(double sec)
    {
        TimeSpan span = TimeSpan.FromSeconds(sec);
        return string.Format("{0:##}{1}{2:00}:{3:00}", span.Hours, span.Hours != 0 ? ":" : "", span.Minutes, span.Seconds);
    }

    public static string TimeFormat(string sec)
    {
        TimeSpan span = TimeSpan.Parse(sec);
        return string.Format("{0:##}{1}{2:00}:{3:00}", span.Hours, span.Hours != 0 ? ":" : "", span.Minutes, span.Seconds);
    }

    public static string ToUppercaseFirst(this string s)
    {
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    //public static string Translate(string key)
    //{
    //    string scenName = Application.loadedLevelName;
    //    Dictionary<string, string> mainPack = TranslationPacks.packs["main"];
    //    if (mainPack.ContainsKey(key))
    //    {
    //        return mainPack[key];
    //    }
    //    else
    //    {
    //        if (TranslationPacks.packs.ContainsKey(scenName))
    //        {
    //            Dictionary<string, string> pack = TranslationPacks.packs[scenName];
    //            if (pack.ContainsKey(key))
    //            {
    //                return pack[key];
    //            }
    //        }
    //    }

    //    return key;
    //}

    public static List<GameObject> CreateChildrenInToList(GameObject parent, int neededChildren, GameObject prefab, bool wantIndexing = true)
    {
        Transform parentCache = parent.transform;
        int parentChildCount = parent.transform.childCount;
        int i;

        if (parentChildCount > neededChildren)
        {
            for (i = parentChildCount - 1; i >= neededChildren; i--)
            {
                //ReSharper disable once AccessToStaticMemberViaDerivedType
                MonoBehaviour.Destroy(parentCache.GetChild(i).gameObject);
            }
        }
        else if (parentChildCount < neededChildren)
        {
            for (i = parentChildCount; i < neededChildren; i++)
            {
                AddChild(parent, prefab);
            }
        }

        List<GameObject> childrenToReturn = new List<GameObject>();
        for (i = 0; i < parentCache.childCount; i++)
        {
            childrenToReturn.Add(parentCache.GetChild(i).gameObject);
            if (wantIndexing)
            {
                childrenToReturn[i].name = i.ToString();
            }
        }

        return childrenToReturn;
    }

    public static GameObject[] CreateChildrenIn(GameObject parent, int needChildren, GameObject child, bool wantIndex = true)
    {
        return CreateChildrenInToList(parent, needChildren, child, wantIndex).ToArray();
    }

    public static T AddMissingComponent<T>(this GameObject go) where T : Component
    {
        T found = go.GetComponent<T>();

        if (found == null)
        {
            return go.AddComponent<T>();
        }

        return found;
    }

    public static bool HasComponent<T>(this GameObject go) where T : Component
    {
        return go.GetComponent<T>() != null;
    }

    public static object TryGet(this Dictionary<string, object> collection, string key, TypeCode expectedTypeData)
    {
        if (collection.ContainsKey(key))
        {
            string value = collection[key].ToString();
            switch (expectedTypeData)
            {
                case TypeCode.Boolean:
                    if (value == "1")
                    {
                        return value == "1";
                    }

                    return Convert.ToBoolean(value);
                case TypeCode.Int32:
                    return int.Parse(value);
                case TypeCode.Single:
                    return float.Parse(value);
                case TypeCode.DateTime:
                    return DateTime.Parse(value);
                case TypeCode.String:
                    return value;
                default:
                    return collection[key];
            }
        }

        switch (expectedTypeData)
        {
            case TypeCode.Boolean:
                return default(bool);
            case TypeCode.Int32:
                return default(int);
            case TypeCode.Single:
                return default(float);
            case TypeCode.DateTime:
                return default(DateTime);
            case TypeCode.String:
                return string.Empty;
            default:
                return null;
        }
    }

    public static T AddChild<T>(Transform parent, T prefab) where T : MonoBehaviour
    {
        if (prefab == null)
        {
            Debug.Log("AddChild() prefab is null.");
            return null;
        }

        if (parent == null)
        {
            return Object.Instantiate(prefab);
        }

        return Object.Instantiate(prefab, parent.position, parent.rotation, parent);
    }

    public static T[] AddChild<T>(Transform parent, T prefab, ushort multiCreateCount) where T : MonoBehaviour
    {
        T[] result = new T[multiCreateCount];
        for (int i = 0; i < multiCreateCount; i++)
        {
            result[i] = AddChild(parent, prefab);
        }

        return result;
    }

    public static GameObject AddChild(GameObject parent, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.Log("AddChild prefab is null");
            return null;
        }

        if (parent == null)
        {
            Debug.Log("AddChild parent is null");
            return null;
        }

        GameObject go = Object.Instantiate(prefab);

        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        return go;
    }

    public static Texture2D ToTexture(this Sprite sprite)
    {
        Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        croppedTexture.SetPixels(sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height));
        croppedTexture.Apply();
        return croppedTexture;
    }

    /// <summary>
    /// Format decimal digits with group separators interval(" ")
    /// <param name="number">Number to format.</param>
    /// <returns>Formatted string.</returns>
    /// </summary>
    public static string FormatMoney(int number)
    {
        string[] suffixes = { "", " K", " M" };

        string formatedNumber;

        if (number >= 10000 && number < 999999) // 10 000...999 999 (10k, 99k, 999k)
        {
            formatedNumber = (number / 1000).ToString();
            return formatedNumber + suffixes[1];
        }

        if (number >= 1000000) // 1 000 000...infinity (1m, 99m 999m)
        {
            formatedNumber = (number / 1000000).ToString();
            return formatedNumber + suffixes[2];
        }

        formatedNumber = number.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = " " });
        return formatedNumber + suffixes[0];
    }

    /// <summary>
    /// Makes given ui gameobject to grayscale by applying a grayscale material
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="applyToChildren"></param>
    public static void ToGrayscale(this Graphic graphic, bool applyToChildren = true)
    {
        ChangeMaterialTo(graphic, GrayScaleMaterial, applyToChildren);
    }

    /// <summary>
    /// Removes the grayscale if applied by simply removing the grayscale material 
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="applyToChildren"></param>
    public static void RemoveGrayscale(this Graphic graphic, bool applyToChildren = true)
    {
        ChangeMaterialTo(graphic, null, applyToChildren);
    }

    /// <summary>
    /// Makes given ui gameobject to grayscale by applying a grayscale material
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="applyToChildren"></param>
    public static Material ToHue(this Graphic graphic, bool applyToChildren = true)
    {
        ChangeMaterialTo(graphic, HueMaterial, applyToChildren);
        return graphic.material;
    }

    /// <summary>
    /// Removes the grayscale if applied by simply removing the grayscale material 
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="applyToChildren"></param>
    public static void RemoveHue(this Graphic graphic, bool applyToChildren = true)
    {
        ChangeMaterialTo(graphic, null, applyToChildren);
    }

    public static void ChangeMaterialTo(Graphic graphic, Material material, bool includeChildren)
    {
        graphic.material = material;

        if (!includeChildren || graphic.transform.childCount == 0)
        {
            return;
        }

        Graphic[] children = graphic.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < children.Length; i++)
        {
            children[i].material = material;
        }
    }

    public static void ChangeMaterialTo(GameObject targetObject, Material material, bool includeChildren)
    {
        if (targetObject.HasComponent<Graphic>() && !targetObject.HasComponent<Text>())
        {
            targetObject.GetComponent<Graphic>().material = material;
        }

        if (!includeChildren || targetObject.transform.childCount == 0)
        {
            return;
        }

        Graphic[] children = targetObject.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].gameObject.HasComponent<Text>())
            {
                continue;
            }

            children[i].material = material;
        }
    }

    public static Color32 ToColor(uint hexVal)
    {
        return new Color32(
            (byte)(hexVal >> 24),
            (byte)(hexVal >> 16),
            (byte)(hexVal >> 8),
            (byte)hexVal);
    }

    public static void DestroyChildren(Transform t)
    {
        while (t.childCount > 0)
        {
            Transform child = t.transform.GetChild(0);
            child.SetParent(null);
            Object.Destroy(child.gameObject);
        }
    }
}