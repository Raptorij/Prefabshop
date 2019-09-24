using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class IgnoreSpawnedPrefabs : Parameter
    {
        public bool value;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            value = EditorGUILayout.Toggle(this.GetType().Name, value);
        }
    }
}