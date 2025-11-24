using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextScriptPrinter))]
public class ReloadButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TextScriptPrinter component = (TextScriptPrinter)target;
        if (GUILayout.Button("Reload"))
            component.Reload();
    }
}