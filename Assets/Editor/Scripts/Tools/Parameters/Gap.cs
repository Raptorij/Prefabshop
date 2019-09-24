using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Gap : Parameter
    {
        public float value = 0f;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.FloatField(this.GetType().Name, value);
        }
    }
}