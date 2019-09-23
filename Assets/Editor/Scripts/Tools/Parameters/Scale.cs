using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Scale : Parameter
    {
        public float value = 0f;

        public override void DrawParameter()
        {
            base.DrawParameter();
            value = EditorGUILayout.FloatField(this.GetType().Name, value);
        }
    }
}