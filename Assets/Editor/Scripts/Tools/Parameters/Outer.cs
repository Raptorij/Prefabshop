using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Outer : Parameter
    {
        public bool value;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.Toggle(this.GetType().Name, value);
        }

        public static Vector3 RandomPointOnCircleEdge(float radius, Vector3 normal)
        {
            var vector2 = Random.insideUnitCircle.normalized * radius;
            return new Vector3(vector2.x, 0, vector2.y);
        }
    }
}
