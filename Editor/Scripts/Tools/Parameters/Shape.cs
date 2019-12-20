using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Shape : Parameter
    {
        private Texture2D texture;
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                if (texture != value)
                {
                    onTextureChange?.Invoke();
                    texture = value;
                }
            }
        }

        private bool invert;
        public bool Invert
        {
            get
            {
                return invert;
            }
            set
            {
                if (invert != value)
                {
                    OnValueChange?.Invoke();
                    invert = value;
                }
            }
        }

        public System.Action onTextureChange;

        public Shape(Type toolType) : base(toolType)
        {
            string path = EditorPrefs.GetString($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", "");
            if (path != "")
            {
                texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            }
        }

        public override void DrawParameterGUI()
        {
            EditorGUI.BeginChangeCheck();
            Texture = EditorGUILayout.ObjectField(this.GetType().Name, Texture, typeof(Texture2D), false) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString($"[Prefabshop] {toolType.Name}.{this.GetType().Name}", AssetDatabase.GetAssetPath(Texture));
            }
            Invert = EditorGUILayout.Toggle("Invert:", Invert);
        }
    }
}