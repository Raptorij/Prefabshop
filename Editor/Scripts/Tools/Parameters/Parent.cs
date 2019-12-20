using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Parent : Parameter
    {
        public Transform value;

        public Parent(Type toolType) : base(toolType)
        {
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.ObjectField(this.GetType().Name, value, typeof(Transform), true) as Transform;
        }
    }
}