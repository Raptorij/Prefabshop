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

        public static ParametersWindow Init(Brush brush)
        {
            var window = Instance;
            window.Show();
            Instance.currentBrush = brush;
            GetWindow<SceneView>().Focus();
            return window;
        }

        private void OnGUI()
        {
            if (currentBrush != null)
            {
                GUILayout.Label(currentBrush.GetType().Name.Replace("Brush", ": Options"), new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
                for (int i = 0; i < currentBrush.parameters.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    currentBrush.parameters[i].DrawParameterGUI();
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                GUILayout.Label("Tool isn't selected", new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
            }
        }
    }
}