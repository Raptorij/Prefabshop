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

        BrushInfo brushInfoCurrent;

        Vector2 scroll;
        private Tool currentTool;
        private System.Type[] possibleTools;
        private List<Tool> cachedTools = new List<Tool>();

        private ParametersWindow parametersWindow;

        void OnEnable()
        {
            SceneView.duringSceneGui += this.OnSceneGUI;
            var types = Assembly.GetExecutingAssembly().GetTypes();
            possibleTools = (from System.Type type in types where type.IsSubclassOf(typeof(Tool)) && type != typeof(SmudgeTool) select type).ToArray();

            var gizmosDrawer = FindObjectOfType<GizmosDrawer>();
            if (gizmosDrawer != null)
            {
                DestroyImmediate(gizmosDrawer);
            }
            gizmosDrawer = new GameObject().AddComponent<GizmosDrawer>();
            gizmosDrawer.gameObject.hideFlags = HideFlags.HideInHierarchy;
            gizmosDrawer.tag = "EditorOnly";
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            DestroyImmediate(FindObjectOfType<GizmosDrawer>().gameObject);
        }        

        private void OnGUI()
        {
            GUILayout.Label("Objects", new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 10));
            EditorGUILayout.BeginHorizontal();
            {
                ObjectsView();
                EditorGUILayout.BeginVertical();
                {
                    if (brushInfoCurrent != null)
                    {
                        GUILayout.Label($"Current Brush: {brushInfoCurrent.name}", EditorStyles.helpBox);
                    }
                    GUI.enabled = brushInfoCurrent != null;
                    if (GUILayout.Button("Save Brush"))
                    {
                        string path = EditorUtility.OpenFolderPanel("Save BrushInfo", "Assets/", "Assets");
                        if (path != string.Empty)
                        {
                            string fullPath = path.Replace(@"\", "/");
                            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                            var bi = ScriptableObjectUtility.CreateAsset<BrushInfo>(assetPath);
                            AssetDatabase.Refresh();
                            EditorUtility.SetDirty(bi);
                            AssetDatabase.SaveAssets();
                            brushInfoCurrent = bi;
                            GetWindow<SceneView>().Focus();
                        }
                    }
                    GUI.enabled = true;
                    if (GUILayout.Button("Load Brush"))
                    {
                        string path = EditorUtility.OpenFilePanel("Сhoose BrushInfo", "Assets/", "asset");
                        if (path != string.Empty)
                        {
                            string fullPath = path.Replace(@"\", "/");
                            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");

                            brushInfoCurrent = AssetDatabase.LoadAssetAtPath(assetPath, typeof(BrushInfo)) as BrushInfo;
                            if (currentTool != null)
                            {
                                SelectTool(currentTool.GetType(), true);
                            }
                            GetWindow<SceneView>().Focus();
                        }
                    }
                    GUI.enabled = brushInfoCurrent != null;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Shortcuts();
            bool haveBrush = blockToggle = currentTool != null;
            GUI.enabled = brushInfoCurrent != null && brushInfoCurrent.brushObjects.Count > 0;
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

            BrushKeyCodeAttribute attribute = brushType.GetCustomAttribute(typeof(BrushKeyCodeAttribute)) as BrushKeyCodeAttribute;
            var brushKey = attribute.keyCode;
            Rect info = new Rect(rect.x + 30, rect.y + 5, rect.width + 80, rect.height);
            GUI.contentColor = Color.black;
            GUI.Label(info, "[" + brushKey.ToString() + "] - " + brushType.Name.Replace("Tool", ""), new GUIStyle("MiniBoldLabel"));
            GUI.contentColor = Color.white;
        }

        void SelectTool(System.Type brushType, bool haveBrush)
        {
            if (!haveBrush || !(currentTool.GetType() == brushType))
            {
                if (haveBrush)
                {
                    currentTool.DeselectTool();
                }
                if (cachedTools.Where(search => search.GetType() == brushType).Count() == 0)
                {
                    var constructor = brushType.GetConstructor(new System.Type[] { typeof(BrushInfo) });
                    currentTool = constructor.Invoke(new object[] { brushInfoCurrent }) as Tool;
                    cachedTools.Add(currentTool);                    
                }
                else
                {
                    currentTool = cachedTools.Find(search => search.GetType() == brushType);
                    currentTool.brushInfo = brushInfoCurrent;                    
                }
                currentTool.SelectTool();
                ParametersWindow.Init(currentTool);
            }
            else
            {
                currentTool.DeselectTool();
                currentTool = null;
                haveBrush = false;
                return;
            }
        }

        void ObjectsView()
        {
            if (brushInfoCurrent != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(Screen.width / 2));
                {
                    scroll = EditorGUILayout.BeginScrollView(scroll);
                    {
                        EditorGUIUtility.labelWidth = 75;
                        SerializedObject serializedObject = new SerializedObject(brushInfoCurrent);
                        var property = serializedObject.FindProperty("brushObjects");
                        serializedObject.Update();
                        EditorGUILayout.PropertyField(property, true);
                        serializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
        }

        void BrushSettingsView(Rect windowRect)
        {
            var fieldsRect = windowRect;            

            //settings.randomizeRotation = EditorGUI.Toggle(fieldsRect, "Random Rotation:", settings.randomizeRotation);
            //settings.randomRotationMin = EditorGUI.Vector3Field(fieldsRect, "", settings.randomRotationMin);
            //settings.randomRotationMax = EditorGUI.Vector3Field(fieldsRect, "", settings.randomRotationMax);            
            
            //settings.toolBar = GUI.Toolbar(fieldsRect, settings.toolBar, settings.menuOptions);

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
                BrushKeyCodeAttribute attribute = possibleTools[i].GetCustomAttribute(typeof(BrushKeyCodeAttribute)) as BrushKeyCodeAttribute;
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