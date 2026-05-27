using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SODatabaseBase), true)]
public class SODatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Run Auto Fill"))
        {
            SODatabaseBase db = (SODatabaseBase)target;
            db.RunAutoFillButton();

            EditorUtility.SetDirty(db);
        }
    }
}