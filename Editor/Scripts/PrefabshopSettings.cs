using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Packages.PrefabshopEditor
{
    public class PrefabshopSettings : EditorWindow
    {
        bool mouseInfo = false;
        bool underMouseInfo = true;

        bool nameInfo = true;
        bool parentInfo = true;
        bool tagInfo = true;
        bool layerInfo = true;
        bool toolInfo = true;
        

        public static void Init(Vector2 pos)
        {
            PrefabshopSettings window = EditorWindow.GetWindow<PrefabshopSettings>();
            GUIContent titleContent = new GUIContent("Prefabshop Settings");
            window.titleContent = titleContent;
            window.Show();
            Rect windowRect = new Rect(pos, window.position.size);
            window.position = windowRect;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            mouseInfo = EditorGUILayout.Foldout(mouseInfo, "Under Mouse Info");
            GUILayout.FlexibleSpace();
            underMouseInfo = EditorGUILayout.Toggle("", underMouseInfo);
            EditorGUILayout.EndHorizontal();
            if (EditorGUILayout.BeginFadeGroup(mouseInfo ? 1f : 0f))
            {
                EditorGUI.indentLevel++;
                GUI.enabled = underMouseInfo;
                nameInfo = EditorGUILayout.Toggle("Name:", nameInfo);
                parentInfo = EditorGUILayout.Toggle("Parent:", parentInfo);
                tagInfo = EditorGUILayout.Toggle("Tag:", tagInfo);
                layerInfo = EditorGUILayout.Toggle("Layer:", layerInfo);
                toolInfo = EditorGUILayout.Toggle("Tool:", toolInfo);
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndVertical();

            GUILayout.Label("Colors");

        }
    }
}
