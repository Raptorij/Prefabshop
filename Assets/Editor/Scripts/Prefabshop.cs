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

        PaintSettings paintSettings;

        private bool workWithOnlyBrushObjects;

        public List<GameObject> brushObjects = new List<GameObject>();

        bool blockToggle;

        BrushInfo brushInfoCurrent;

        Vector2 scroll;
        private Brush currentBrush;
        private System.Type[] possibleBrushes;
        private List<Brush> cachedBrushes = new List<Brush>();

        private ParametersWindow parametersWindow;

        void OnEnable()
        {
            SceneView.duringSceneGui += this.OnSceneGUI;
            var types = Assembly.GetExecutingAssembly().GetTypes();
            possibleBrushes = (from System.Type type in types where type.IsSubclassOf(typeof(Brush)) select type).ToArray();
            paintSettings = new PaintSettings();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
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
                            if (currentBrush != null)
                            {
                                var brushType = currentBrush.GetType();
                                var constructor = brushType.GetConstructor(new System.Type[] { typeof(BrushInfo), typeof(PaintSettings) });
                                currentBrush = constructor.Invoke(new object[] { brushInfoCurrent, paintSettings }) as Brush;
                            }
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
            var currentSettings = brushInfoCurrent != null ? brushInfoCurrent.settings : paintSettings;
            Shortcuts(currentSettings);
            bool haveBrush = blockToggle = currentBrush != null;
            if (haveBrush)
            {
                currentBrush.CastBrush();
            }
            GUI.enabled = brushInfoCurrent != null && brushInfoCurrent.brushObjects.Count > 0;
            Handles.BeginGUI();
            {
                Rect settingsInfoRect = new Rect(1, 1, 35, 30 * possibleBrushes.Length + 5);
                GUI.Box(settingsInfoRect, "", new GUIStyle("HelpBox"));

                Rect buttonRect = new Rect(5, 5, 25, 25);
                for (int i = 0; i < possibleBrushes.Length; i++)
                {
                    DrawBrush(buttonRect, possibleBrushes[i], haveBrush);
                    buttonRect.y += 30;
                    if (haveBrush && currentBrush == null)
                    {
                        break;
                    }
                }
            }
            Handles.EndGUI();
            Tools.hidden = blockToggle;
        }

        void DrawBrush(Rect rect, System.Type brushType, bool haveBrush)
        {
            GUI.backgroundColor = (haveBrush && (currentBrush.GetType() == brushType)) ? Color.gray : Color.white;
            Texture2D brushIcon = Resources.Load(brushType.Name) as Texture2D;
            if (GUI.Button(rect, brushIcon))
            {
                SelectTool(brushType, haveBrush);
            }
            GUI.backgroundColor = Color.white;

            BrushKeyCodeAttribute attribute = brushType.GetCustomAttribute(typeof(BrushKeyCodeAttribute)) as BrushKeyCodeAttribute;
            var brushKey = attribute.keyCode;
            Rect info = new Rect(rect.x + 30, rect.y + 5, rect.width + 80, rect.height);
            GUI.Label(info, "[" + brushKey.ToString() + "] - " + brushType.Name.Replace("Brush", ""));
        }

        void SelectTool(System.Type brushType, bool haveBrush)
        {
            if (!haveBrush || !(currentBrush.GetType() == brushType))
            {                
                if (cachedBrushes.Where(search => search.GetType() == brushType).Count() == 0)
                {
                    var constructor = brushType.GetConstructor(new System.Type[] { typeof(BrushInfo), typeof(PaintSettings) });
                    currentBrush = constructor.Invoke(new object[] { brushInfoCurrent, paintSettings }) as Brush;
                    cachedBrushes.Add(currentBrush);
                }
                else
                {
                    currentBrush = cachedBrushes.Find(search => search.GetType() == brushType);
                }
                ParametersWindow.Init(currentBrush);
            }
            else
            {
                currentBrush = null;
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

        void BrushSettingsView(Rect windowRect, PaintSettings settings)
        {
            var fieldsRect = windowRect;            

            settings.randomizeRotation = EditorGUI.Toggle(fieldsRect, "Random Rotation:", settings.randomizeRotation);
            settings.randomRotationMin = EditorGUI.Vector3Field(fieldsRect, "", settings.randomRotationMin);
            settings.randomRotationMax = EditorGUI.Vector3Field(fieldsRect, "", settings.randomRotationMax);            
            
            settings.toolBar = GUI.Toolbar(fieldsRect, settings.toolBar, settings.menuOptions);

        }

        void Shortcuts(PaintSettings settings)
        {
            var e = Event.current;
            //if (blockToggle)
            //{
            //    if (e.delta.y != 0 && e.isScrollWheel && (e.shift || e.control))
            //    {
            //        if (e.shift && !e.control)
            //        {
            //            settings.count += (int)Mathf.Sign(e.delta.y);
            //            this.Repaint();
            //        }
            //        else if (e.control && !e.shift)
            //        {
            //            float delta = e.delta.y;
            //            if (delta % 1 == 0)
            //            {
            //                settings.radius *= delta > 0f ? 1.1f : 0.9f;
            //            }
            //            else
            //            {
            //                settings.radius += delta;
            //            }
            //            this.Repaint();
            //        }
            //        else if (e.shift && e.control)
            //        {
            //            float delta = e.delta.y;
            //            if (delta % 1 == 0)
            //            {
            //                settings.gap += Mathf.Sign(delta) *  0.9f;
            //            }
            //            else
            //            {
            //                settings.gap += delta;
            //            }
            //            this.Repaint();
            //        }
            //        if (e.type == EventType.ScrollWheel)
            //        {
            //            e.Use();
            //        }
            //    }

            //    if (e.type != EventType.KeyDown)
            //    {
            //        return;
            //    }

            //    if (e.character == '\t')
            //    {
            //        settings.toolBar++;
            //        if (settings.toolBar > settings.menuOptions.Length - 1)
            //        {
            //            settings.toolBar = 0;
            //        }
            //        e.Use();
            //        this.Repaint();
            //    }
            //}

            if (e.type != EventType.KeyDown)
            {
                return;
            }

            for (int i = 0; i < possibleBrushes.Length; i++)
            {
                BrushKeyCodeAttribute attribute = possibleBrushes[i].GetCustomAttribute(typeof(BrushKeyCodeAttribute)) as BrushKeyCodeAttribute;
                var brushKey = attribute.keyCode;
                if (e.keyCode == brushKey)
                {
                    SelectTool(possibleBrushes[i], currentBrush != null);
                    blockToggle = currentBrush != null;
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