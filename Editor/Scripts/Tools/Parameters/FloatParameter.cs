using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Packages.PrefabshopEditor
{
    public class FloatParameter : Parameter
    {
        public string name;
        public float value;

        public FloatParameter(Type toolType) : base(toolType)
        {
            //Load parameter values
            Load();
        }

        public FloatParameter(Type toolType, string name, int id) : base(toolType)
        {
            this.name = name;
            this.Identifier = id;
            //Load parameter values
            Load();
        }

        void Load()
        {
            value = EditorPrefs.GetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.{name}.{Identifier}", value);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            //Draw changeable values
            value = EditorGUILayout.FloatField(name + ":", value);
            value = Mathf.Clamp(value, 0, Mathf.Infinity);
            if (EditorGUI.EndChangeCheck())
            {
                //Save changes of parameter values
                EditorPrefs.SetFloat($"[Prefabshop] {toolType.Name}.{this.GetType().Name}.{name}.{Identifier}", value);
            }
        }
    }
}