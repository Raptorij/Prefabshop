using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

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

        private System.Type[] tools;
        Color paint;
        Color remove;
        Color other;

        public static void Init(Vector2 pos)
        {
            PrefabshopSettings window = EditorWindow.GetWindow<PrefabshopSettings>();
            window.minSize = new Vector2(320, 420);
            GUIContent titleContent = new GUIContent("Prefabshop Settings");
            window.titleContent = titleContent;
            window.Show();
            Rect windowRect = new Rect(pos, window.position.size);
            window.position = windowRect;

            var types = Assembly.GetExecutingAssembly().GetTypes();
            window.tools = (from System.Type type in types where type.IsSubclassOf(typeof(Tool)) select type).ToArray();

            window.paint = ToolColorAttribute.GetColor(ToolColorAttribute.ToolUseType.Paint);
            window.remove = ToolColorAttribute.GetColor(ToolColorAttribute.ToolUseType.Remove);
            window.other = ToolColorAttribute.GetColor(ToolColorAttribute.ToolUseType.Other);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            mouseInfo = EditorGUILayout.Foldout(mouseInfo, "Under Mouse Info");
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            underMouseInfo = EditorGUILayout.Toggle("", EditorPrefs.GetBool("[Prefabshop] underMouseInfo", underMouseInfo));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("[Prefabshop] underMouseInfo", underMouseInfo);
            }

            EditorGUILayout.EndHorizontal();
            if (EditorGUILayout.BeginFadeGroup(mouseInfo ? 1f : 0f))
            {
                EditorGUI.indentLevel++;
                GUI.enabled = underMouseInfo;

                EditorGUI.BeginChangeCheck();
                nameInfo = EditorGUILayout.Toggle("Name:", nameInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("[Prefabshop] nameInfo", nameInfo);
                }

                EditorGUI.BeginChangeCheck();
                parentInfo = EditorGUILayout.Toggle("Parent:", parentInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("[Prefabshop] parentInfo", parentInfo);
                }

                EditorGUI.BeginChangeCheck();
                tagInfo = EditorGUILayout.Toggle("Tag:", tagInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("[Prefabshop] tagInfo", tagInfo);
                }

                EditorGUI.BeginChangeCheck();
                layerInfo = EditorGUILayout.Toggle("Layer:", layerInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("[Prefabshop] layerInfo", layerInfo);
                }

                EditorGUI.BeginChangeCheck();
                toolInfo = EditorGUILayout.Toggle("Tool:", toolInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("[Prefabshop] toolInfo", toolInfo);
                }

                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("Colors");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.BeginChangeCheck();
                    paint = EditorGUILayout.ColorField("Paint:", paint);
                    DrawColorSetting(ToolColorAttribute.ToolUseType.Paint);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ToolColorAttribute.SaveColor(ToolColorAttribute.ToolUseType.Paint, paint);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.BeginChangeCheck();
                    remove = EditorGUILayout.ColorField("Remove:", remove);
                    DrawColorSetting(ToolColorAttribute.ToolUseType.Remove);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ToolColorAttribute.SaveColor(ToolColorAttribute.ToolUseType.Remove, remove);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.BeginChangeCheck();
                    other = EditorGUILayout.ColorField("Other:", other);
                    DrawColorSetting(ToolColorAttribute.ToolUseType.Other);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ToolColorAttribute.SaveColor(ToolColorAttribute.ToolUseType.Other, other);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawColorSetting(ToolColorAttribute.ToolUseType toolUseType)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (tools != null)
                {
                    for (int i = 0; i < tools.Length; i++)
                    {
                        ToolColorAttribute attribute = tools[i].GetCustomAttribute(typeof(ToolColorAttribute)) as ToolColorAttribute;
                        if (attribute != null && toolUseType == attribute.toolUseType)
                        {
                            Texture2D brushIcon = Resources.Load(tools[i].Name) as Texture2D;
                            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                            GUILayout.Label(new GUIContent(brushIcon), GUILayout.Width(20), GUILayout.Height(20));
                            GUI.contentColor = Color.white;
                        }
                    }
                }
                else
                {
                    var types = Assembly.GetExecutingAssembly().GetTypes();
                    tools = (from System.Type type in types where type.IsSubclassOf(typeof(Tool)) select type).ToArray();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
