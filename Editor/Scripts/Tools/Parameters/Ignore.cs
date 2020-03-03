using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Ignore : Parameter
    {
        public LayerMask layer;

        public int ignorePrefabsId;
        string[] ignorePrefabsTabs = new string[] { "Selected Prefabs", "All in PrefabSet", "All Prefabs" };
        public System.Action<int> onChangeToolBar;

        public Ignore(Type toolType) : base(toolType)
        {
            layer = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", layer);
            ignorePrefabsId = EditorPrefs.GetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", ignorePrefabsId);
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            GUILayout.Label("Ignore Options");
            EditorGUI.BeginChangeCheck();
            layer = LayerMaskField("Layer:", layer);
            GUILayout.Label("Prefabs Ignore");
            ignorePrefabsId = GUILayout.Toolbar(ignorePrefabsId, ignorePrefabsTabs);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", layer);
                EditorPrefs.SetInt($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", ignorePrefabsId);
                onChangeToolBar?.Invoke(ignorePrefabsId);
            }
        }

        static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != string.Empty)
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }
            layerMask.value = mask;
            return layerMask;
        }
    }
}