using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Scale : Parameter
    {
        public bool randomScale;
        public float minValue = 1f;
        public float maxValue = 10f;

        public Scale(Type toolType) : base(toolType)
        {
            randomScale = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.randomScale", randomScale ? 1 : 0) == 1;
            minValue = EditorPrefs.GetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.minValue", minValue);
            maxValue = EditorPrefs.GetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.maxValue", maxValue);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            GUI.enabled = randomScale = EditorGUILayout.Toggle("Random Scale:", randomScale);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.randomScale", randomScale ? 1 : 0);
            }

            EditorGUI.BeginChangeCheck();
            minValue = EditorGUILayout.FloatField("Min Val:", minValue);
            maxValue = EditorGUILayout.FloatField("Max Val:", maxValue);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, 0f, 100f);
            GUI.enabled = true;
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.minValue", minValue);
                EditorPrefs.SetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.maxValue", maxValue);
            }
        }
    }
}