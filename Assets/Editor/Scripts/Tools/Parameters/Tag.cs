using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Tag : Parameter
    {
        public string value = "Untagged";

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.TagField(this.GetType().Name, value);
        }
    }
}