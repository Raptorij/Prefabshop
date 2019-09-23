using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Shape : Parameter
    {
        public Texture2D texture;

        public override void DrawParameter()
        {
            texture = EditorGUILayout.ObjectField(this.GetType().Name, texture, typeof(Texture2D), false) as Texture2D;
        }
    }
}