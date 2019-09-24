using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Packages.PrefabshopEditor
{
    public class Count : Parameter
    {
        public int value = 1;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.IntField(this.GetType().Name, value);
        }
    }
}
