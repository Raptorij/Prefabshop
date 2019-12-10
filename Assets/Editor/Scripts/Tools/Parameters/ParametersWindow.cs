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

        public Tool currentBrush;
        private Vector2 scrollView;

        public static ParametersWindow Init(Tool brush)
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
                GUILayout.Label(currentBrush.GetType().Name.Replace("Tool", ": Options"), new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
                scrollView = EditorGUILayout.BeginScrollView(scrollView);
                {
                    for (int i = 0; i < currentBrush.parameters.Count; i++)
                    {
                        if (!currentBrush.parameters[i].Hidden)
                        {
                            GUI.enabled = currentBrush.parameters[i].Enable;
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            currentBrush.parameters[i].DrawParameterGUI();
                            EditorGUILayout.EndVertical();
                            GUI.enabled = true;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Tool isn't selected", new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
            }
        }
    }
}