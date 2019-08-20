using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BetweenBase), true)]
public class BetweenBaseEditor : Editor
{
    private bool stateHideInInspector;
    private bool editorActiveUpdate = false;



    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(this.serializedObject,
            "Duration",
            "style",
            "DeactivateOn",
            "CurveEvaluation",
            "AnimationCurve",
            "OnFinish",
            "m_Script");
        GUILayout.Space(7);

        BetweenBase trans = this.target as BetweenBase;
        trans.Duration = EditorGUILayout.FloatField("Duration", trans.Duration);
        trans.style = (BetweenBase.Style)EditorGUILayout.EnumPopup("Style", trans.style);
        trans.DeactivateOn = (BetweenBase.Deactivate)EditorGUILayout.EnumPopup("Deactivate On", trans.DeactivateOn);
        trans.CurveEvaluation = GUILayout.Toggle(trans.CurveEvaluation, "Curve Evaluation");

        if (trans.CurveEvaluation)
        {
            trans.AnimationCurve = EditorGUILayout.CurveField("Animation Curve", trans.AnimationCurve);
        }

        GUILayout.Space(7);
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("OnFinish"), true);

        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        Color foldoutColor = Color.white;
        foldoutStyle.fontStyle = FontStyle.Bold;
        foldoutStyle.normal.textColor = foldoutColor;
        foldoutStyle.fontSize = 14;
        foldoutStyle.onNormal.textColor = foldoutColor;
        foldoutStyle.hover.textColor = foldoutColor;
        foldoutStyle.onHover.textColor = foldoutColor;
        foldoutStyle.focused.textColor = foldoutColor;
        foldoutStyle.onFocused.textColor = foldoutColor;
        foldoutStyle.active.textColor = foldoutColor;
        foldoutStyle.onActive.textColor = foldoutColor;
        this.stateHideInInspector = EditorGUILayout.Foldout(this.stateHideInInspector, "Debug transition tools", true, foldoutStyle);

        if (this.stateHideInInspector)
        {
            GUILayoutOption guiLayoutOptionWidth = GUILayout.Width((Screen.width - 50) / 5f);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("|◄", guiLayoutOptionWidth))
            {
                this.editorActiveUpdate = false;
                EditorApplication.update -= trans.Update;
                trans.ResetToBeginning();
            }

            if (GUILayout.Button("◄", guiLayoutOptionWidth))
            {
                if (!this.editorActiveUpdate)
                {
                    this.editorActiveUpdate = true;
                    EditorApplication.update += trans.Update;
                }

                trans.PlayReverse();
            }

            if (GUILayout.Button("||", guiLayoutOptionWidth))
            {
                this.editorActiveUpdate = false;
                EditorApplication.update -= trans.Update;
            }

            if (GUILayout.Button("►", guiLayoutOptionWidth))
            {
                if (!this.editorActiveUpdate)
                {
                    this.editorActiveUpdate = true;
                    EditorApplication.update += trans.Update;
                }

                trans.PlayForward();
            }

            if (GUILayout.Button("►|", guiLayoutOptionWidth))
            {
                this.editorActiveUpdate = false;
                EditorApplication.update -= trans.Update;
                trans.ResetToEnd();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        EditorUtility.SetDirty(this.target);
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        this.serializedObject.ApplyModifiedProperties();
    }

    public void OnDisable()
    {
        EditorApplication.update -= (this.target as BetweenBase).Update;
    }
}
