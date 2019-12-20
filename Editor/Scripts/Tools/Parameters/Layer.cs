using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Layer : Parameter
    {
        public int value;

        public Layer(Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.LayerField(this.GetType().Name, value);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
            }
        }
    }
}