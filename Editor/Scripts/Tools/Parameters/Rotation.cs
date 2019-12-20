using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Rotation : Parameter
    {
        public bool randomRotation;

        public Rotation(Type toolType) : base(toolType)
        {
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            GUI.enabled = randomRotation = EditorGUILayout.Toggle("Random Rotation:", randomRotation);
            //minValue = EditorGUILayout.FloatField("Min Val:", minValue);
            //maxValue = EditorGUILayout.FloatField("Max Val:", maxValue);
            //EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, 0f, 100f);
            GUI.enabled = true;
        }
    }
}
