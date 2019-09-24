using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Layer : Parameter
    {
        public int value;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.LayerField(this.GetType().Name, value);
        }
    }
}