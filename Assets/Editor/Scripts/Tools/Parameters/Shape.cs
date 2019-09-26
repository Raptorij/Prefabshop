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
                    OnTextureChange?.Invoke();
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

        public System.Action OnTextureChange;

        public override void DrawParameterGUI()
        {
            Texture = EditorGUILayout.ObjectField(this.GetType().Name, Texture, typeof(Texture2D), false) as Texture2D;
            Invert = EditorGUILayout.Toggle("Invetr:", Invert);
        }
    }
}