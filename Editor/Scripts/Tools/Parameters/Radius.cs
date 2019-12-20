using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Radius : Parameter
    {
        public float value = 10f;

        public Radius(Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = Mathf.Clamp(EditorGUILayout.FloatField(this.GetType().Name, value),0, Mathf.Infinity);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
            }
        }
    }
}