using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;

namespace Packages.PrefabshopEditor
{
    public class Prefabshop : EditorWindow
    {
        [MenuItem("Tools/Prefabshop")]
        static void Init()
        {
            EditorWindow.GetWindow<Prefabshop>().Show();
        }                

        bool blockToggle;

        Vector2 scrollView;
        private Tool currentTool;
        private System.Type[] possibleTools;
        private List<Tool> cachedTools = new List<Tool>();        

        void OnEnable()
        {
            SceneView.duringSceneGui += this.OnSceneGUI;
            var types = Assembly.GetExecutingAssembly().GetTypes();
            possibleTools = (from System.Type type in types where type.IsSubclassOf(typeof(Tool)) && type != typeof(SmudgeTool) select type).ToArray();           
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }        

        private void OnGUI()
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            {
                if (currentTool != null)
                {
                    GUILayout.Label(currentTool.GetType().Name.Replace("Tool", ": Options"), new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
            
                    for (int i = 0; i < currentTool.parameters.Count; i++)
                    {
                        if (!currentTool.parameters[i].Hidden)
                        {
                            GUI.enabled = currentTool.parameters[i].Enable;
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            currentTool.parameters[i].DrawParameterGUI();
                            EditorGUILayout.EndVertical();
                            GUI.enabled = true;
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Tool isn't selected", new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Shortcuts();
            bool haveBrush = blockToggle = currentTool != null;
            Handles.BeginGUI();
            {
                Rect settingsInfoRect = new Rect(1, 1, 35, 30 * possibleTools.Length + 5);
                GUI.Box(settingsInfoRect, "", new GUIStyle("HelpBox"));

                Rect buttonRect = new Rect(5, 5, 25, 25);
                for (int i = 0; i < possibleTools.Length; i++)
                {
                    DrawToolGUI(buttonRect, possibleTools[i], haveBrush);
                    buttonRect.y += 30;
                    if (haveBrush && currentTool == null)
                    {
                        break;
                    }
                }
            }
            Handles.EndGUI();
            currentTool?.CastBrush();
            Tools.hidden = blockToggle;
        }

        void DrawToolGUI(Rect rect, System.Type brushType, bool haveBrush)
        {
            GUI.backgroundColor = (haveBrush && (currentTool.GetType() == brushType)) ? Color.gray : Color.white;
            Texture2D brushIcon = Resources.Load(brushType.Name) as Texture2D;
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (GUI.Button(rect, brushIcon))
            {
                SelectTool(brushType, haveBrush);
                Event.current.Use();
            }
            GUI.backgroundColor = Color.white;

            ToolKeyCodeAttributeAttribute attribute = brushType.GetCustomAttribute(typeof(ToolKeyCodeAttributeAttribute)) as ToolKeyCodeAttributeAttribute;
            var brushKey = attribute.keyCode;
            Rect info = new Rect(rect.x + 30, rect.y + 5, rect.width + 80, rect.height);
            GUI.contentColor = Color.black;
            GUI.Label(info, "[" + brushKey.ToString() + "] - " + brushType.Name.Replace("Tool", ""), new GUIStyle("MiniBoldLabel"));
            GUI.contentColor = Color.white;
        }

        void SelectTool(System.Type toolType, bool haveBrush)
        {
            if (!haveBrush || !(currentTool.GetType() == toolType))
            {
                if (haveBrush)
                {
                    currentTool.DeselectTool();
                }
                if (cachedTools.Where(search => search.GetType() == toolType).Count() == 0)
                {
                    var constructor = toolType.GetConstructor(new System.Type[] {  });
                    currentTool = constructor.Invoke(new object[] { }) as Tool;
                    cachedTools.Add(currentTool);                    
                }
                else
                {
                    currentTool = cachedTools.Find(search => search.GetType() == toolType);
                }
                currentTool.SelectTool();
            }
            else
            {
                currentTool.DeselectTool();
                currentTool = null;
                haveBrush = false;
                return;
            }
            Repaint();
        }

        void Shortcuts()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown)
            {
                return;
            }
            for (int i = 0; i < possibleTools.Length; i++)
            {
                ToolKeyCodeAttributeAttribute attribute = possibleTools[i].GetCustomAttribute(typeof(ToolKeyCodeAttributeAttribute)) as ToolKeyCodeAttributeAttribute;
                var brushKey = attribute.keyCode;
                if (e.keyCode == brushKey)
                {
                    SelectTool(possibleTools[i], currentTool != null);
                    blockToggle = currentTool != null;
                    if (blockToggle)
                    {
                        Selection.activeGameObject = null;
                    }
                    e.Use();
                }
            }
        }
    }
}