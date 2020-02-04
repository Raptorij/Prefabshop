using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Step : Parameter
    {
        public float value = 1f;

        public Step(Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            value = Mathf.Clamp(EditorGUILayout.FloatField(this.GetType().Name, value), 0, Mathf.Infinity);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", value);
            }
        }

        public Vector3 GetSnappedPosition(Vector3 prevPos, Vector3 currentPos)
        {
            var result = prevPos;
            var heading = currentPos - prevPos;
            float distance = heading.magnitude;
            var direction = heading / distance;
            result = prevPos + direction * value;
            return result;
        }
    }
}