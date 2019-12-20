using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Outer : Parameter
    {
        public bool value;

        public Outer(System.Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value ? 1 : 0) == 1;
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.Toggle(this.GetType().Name, value);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value ? 1 : 0);
            }
        }

        public static Vector3 RandomPointOnCircleEdge(float radius, Vector3 normal)
        {
            var vector2 = Random.insideUnitCircle.normalized * radius;
            return new Vector3(vector2.x, 0, vector2.y);
        }
    }
}
