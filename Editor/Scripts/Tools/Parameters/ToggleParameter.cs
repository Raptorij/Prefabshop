using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class ToggleParameter : Parameter
    {
        public string toggleName;
        public bool value;

        public ToggleParameter(Type toolType) : base(toolType)
        {
            string saveId = $"[Prefabshop] {toolType.Name}.{this.GetType().Name}.{toggleName}.{Identifier}";
            value = EditorPrefs.GetBool(saveId, false);
        }

        public ToggleParameter(Type toolType, string name, int id) : base(toolType)
        {
            this.toggleName = name;
            this.Identifier = id;

            string saveId = $"[Prefabshop] {toolType.Name}.{this.GetType().Name}.{toggleName}.{Identifier}";
            value = EditorPrefs.GetBool(saveId, false);
        }

        public override void DrawParameterGUI()
        {
            EditorGUI.BeginChangeCheck();            
            value = EditorGUILayout.Toggle(toggleName + ":", value);
            if (EditorGUI.EndChangeCheck())
            {
                string saveId = $"[Prefabshop] {toolType.Name}.{this.GetType().Name}.{toggleName}.{Identifier}";
                EditorPrefs.SetBool(saveId, value);
            }
        }
    }
}