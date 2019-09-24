using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class FirstObjectFilter : Parameter
    {
        public bool value;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.Toggle(this.GetType().Name, value);
        }
    }
}