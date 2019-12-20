using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class PrefabSelector : Parameter
    {
        public PrefabSelector(System.Type toolType) : base(toolType)
        {

        }

        public bool haveButton;
        public string buttonName;
        public GameObject selectedPrefab;
        public System.Action onButtonClick;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            selectedPrefab = EditorGUILayout.ObjectField("Prefab:", selectedPrefab, typeof(GameObject), false) as GameObject;
            if (GUILayout.Button(buttonName))
            {
                onButtonClick?.Invoke();
            }
        }
    }
}
