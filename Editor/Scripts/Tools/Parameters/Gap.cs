using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Gap : Parameter
    {
        public float value = 0f;

        public Gap(Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(this.GetType().Name, value);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
            }
        }
    }
}