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

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            GUI.enabled = randomScale = EditorGUILayout.Toggle("Random Scale:", randomScale);
            minValue = EditorGUILayout.FloatField("Min Val:", minValue);
            maxValue = EditorGUILayout.FloatField("Max Val:", maxValue);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, 0f, 100f);
            GUI.enabled = true;
        }
    }
}