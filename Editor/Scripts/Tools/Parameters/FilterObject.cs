using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class FilterObject : Parameter
    {
        public GameObject value;

        public FilterObject(Type toolType) : base(toolType)
        {
            
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.ObjectField(this.GetType().Name, value, typeof(GameObject), true) as GameObject;
        }
    }
}