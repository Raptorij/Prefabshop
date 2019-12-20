using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Tag : Parameter
    {
        public string value = "Untagged";

        public Tag(Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetString($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.TagField(this.GetType().Name, value);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
            }
        }
    }
}