using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class IgnoreSpawnedPrefabs : Parameter
    {
        public bool value;

        public IgnoreSpawnedPrefabs(Type toolType) : base(toolType)
        {
            value = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", 0) == 1;
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
    }
}