using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RaycastTarget : Image
{
    protected void Reset()
    {
        this.color = new Color32(0, 0, 0, 2);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaycastTarget)), DisallowMultipleComponent]
public class RaycastTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Image trans = this.target as Image;
        //trans.raycastTarget = EditorGUILayout.Toggle("Raycast", trans.raycastTarget);
    }
}
#endif