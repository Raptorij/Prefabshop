using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Radius : Parameter
    {
        public float value = 10f;

        public override void DrawParameter()
        {
            base.DrawParameter();
            value = EditorGUILayout.FloatField(this.GetType().Name, value);
        }
    }
}