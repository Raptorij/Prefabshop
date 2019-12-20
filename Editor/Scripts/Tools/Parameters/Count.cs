using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Packages.PrefabshopEditor
{
    public class Count : Parameter
    {
        public int value = 1;

        public Count(System.Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = (int)Mathf.Clamp(EditorGUILayout.IntField(this.GetType().Name, value), 1, Mathf.Infinity);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
            }
        }
    }
}
