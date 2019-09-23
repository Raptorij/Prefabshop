using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Packages.PrefabshopEditor
{
    public class ParametersWindow : EditorWindow
    {
        public static ParametersWindow Instance
        {
            get
            {
                return GetWindow<ParametersWindow>();
            }
        }

        public Brush currentBrush;

        public static void Init(Brush brush)
        {
            GetWindow<ParametersWindow>(brush.GetType().Name, false).Show();
            Instance.currentBrush = brush;
            Instance.name = brush.GetType().Name;
            GetWindow<SceneView>().Focus();
        }

        private void OnGUI()
        {
            if (currentBrush != null)
            {
                for (int i = 0; i < currentBrush.parameters.Count; i++)
                {
                    currentBrush.parameters[i].DrawParameter();
                }
            }
        }
    }
}