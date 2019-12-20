using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class SelectToolBar : Parameter
    {
        public int idSelect;
        public string[] toolBar;

        public System.Action<int> onChangeToolBar;

        public SelectToolBar(Type toolType) : base(toolType)
        {
            idSelect = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", idSelect);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUI.BeginChangeCheck();
            idSelect = GUILayout.Toolbar(idSelect, toolBar);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", idSelect);
                onChangeToolBar?.Invoke(idSelect);
            }
        }
    }
}