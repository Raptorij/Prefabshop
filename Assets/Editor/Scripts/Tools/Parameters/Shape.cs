using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Shape : Parameter
    {
        public Texture2D texture;
        public bool invert;

        public override void DrawParameterGUI()
        {
            texture = EditorGUILayout.ObjectField(this.GetType().Name, texture, typeof(Texture2D), false) as Texture2D;
            invert = EditorGUILayout.Toggle("Invetr:", invert);
        }
    }
}