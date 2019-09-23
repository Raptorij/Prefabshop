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
            HelpInfo();
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

                Rect parametersRect = new Rect(10, 10, 240, EditorGUIUtility.singleLineHeight);

                if (haveBrush)
                {
                    ParametersWindow.Init(currentBrush);
                }
                //BrushSettingsView(settingsInfoRect, currentSettings);

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
                var constructor = brushType.GetConstructor(new System.Type[] { typeof(BrushInfo), typeof(PaintSettings) });
                currentBrush = constructor.Invoke(new object[] { brushInfoCurrent, paintSettings }) as Brush;
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
            fieldsRect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(fieldsRect, "Paint Settings" + (brushInfoCurrent != null ? ": " + brushInfoCurrent.name : ""), new GUIStyle("HelpBox"));
            EditorGUIUtility.fieldWidth = 60;
            fieldsRect.width = 1020 / 4f;
            fieldsRect.width -= 60f;
            fieldsRect.x += 5;
            fieldsRect.y += 5;

            EditorGUIUtility.labelWidth = 100;
            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            settings.radius = EditorGUI.FloatField(fieldsRect, "Radius:", settings.radius);
            settings.radius = settings.radius >= 0 ? settings.radius : 0;

            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            settings.count = EditorGUI.IntField(fieldsRect, "Object Count:", settings.count);
            settings.count = settings.count >= 1 ? settings.count : 1;

            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            settings.gap = EditorGUI.FloatField(fieldsRect, "Gap:", settings.gap);

            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            settings.targetTag = EditorGUI.TagField(fieldsRect, "Tag:", settings.targetTag);

            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            settings.targetLayer = EditorGUI.LayerField(fieldsRect, "Layer:", settings.targetLayer);



            fieldsRect.x += 205;
            fieldsRect.y = EditorGUIUtility.singleLineHeight + 10;
            settings.randomizeScale = EditorGUI.Toggle(fieldsRect, "Random Scale:", settings.randomizeScale);
            GUI.enabled = settings.randomizeScale;
            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUIUtility.labelWidth = 30;
            fieldsRect.width /= 2f;
            fieldsRect.width -= 5f;
            //fieldsRect.width -= 5;
            settings.randomScaleMin = EditorGUI.FloatField(fieldsRect, "Min:", settings.randomScaleMin);
            settings.randomScaleMin = settings.randomScaleMin >= 0 ? settings.randomScaleMin : 0;
            fieldsRect.x += 120;
            settings.randomScaleMax = EditorGUI.FloatField(fieldsRect, "Max:", settings.randomScaleMax);
            settings.randomScaleMax = settings.randomScaleMax >= 0 ? settings.randomScaleMax : 0;
            fieldsRect.width += 5f;
            settings.randomScaleMax = settings.randomScaleMin > settings.randomScaleMax ? settings.randomScaleMin : settings.randomScaleMax;
            GUI.enabled = false;

            EditorGUIUtility.labelWidth = 120;
            fieldsRect.width = 240;
            fieldsRect.x += 105;
            fieldsRect.y = EditorGUIUtility.singleLineHeight + 10;
            settings.randomizeRotation = EditorGUI.Toggle(fieldsRect, "Random Rotation:", settings.randomizeRotation);
            GUI.enabled = settings.randomizeRotation;
            EditorGUIUtility.labelWidth = 30;
            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            fieldsRect.y += 5f;
            GUI.Label(fieldsRect, "Minimum:");
            fieldsRect.width -= 70;
            fieldsRect.x += 70;
            settings.randomRotationMin = EditorGUI.Vector3Field(fieldsRect, "", settings.randomRotationMin);
            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            fieldsRect.width += 70;
            fieldsRect.x -= 70;
            fieldsRect.y += 5f;
            GUI.Label(fieldsRect, "Minimum:");
            fieldsRect.width -= 70;
            fieldsRect.x += 70;
            settings.randomRotationMax = EditorGUI.Vector3Field(fieldsRect, "", settings.randomRotationMax);

            EditorGUIUtility.labelWidth = 85;
            fieldsRect.x -= 70;
            fieldsRect.width = 140;
            fieldsRect.y += 5f;
            fieldsRect.y += EditorGUIUtility.singleLineHeight;

            GUI.enabled = true;
            GUI.Label(fieldsRect, "Up Direction:");
            fieldsRect.x += 80;
            fieldsRect.width += 45;
            settings.toolBar = GUI.Toolbar(fieldsRect, settings.toolBar, settings.menuOptions);

            fieldsRect.width = 225;
            fieldsRect.x += 200;
            fieldsRect.y = EditorGUIUtility.singleLineHeight + 10;

            settings.targetParent = EditorGUI.ObjectField(fieldsRect, "Parent:", settings.targetParent, typeof(Transform), true) as Transform;

            fieldsRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            EditorGUIUtility.labelWidth = 100;
            settings.ignoringLayer = LayerMaskField(fieldsRect, "Ignored Layers:", settings.ignoringLayer);

            EditorGUIUtility.labelWidth = 120;
            fieldsRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            settings.firstObjectFilter = EditorGUI.Toggle(fieldsRect, "First Object Filter:", settings.firstObjectFilter);

            EditorGUIUtility.labelWidth = 85;
            fieldsRect.y += EditorGUIUtility.singleLineHeight;
            settings.filterObject = EditorGUI.ObjectField(fieldsRect, "Filter Object:", settings.filterObject, typeof(GameObject), true) as GameObject;
        }

        void Shortcuts(PaintSettings settings)
        {
            var e = Event.current;
            if (blockToggle)
            {
                if (e.delta.y != 0 && e.isScrollWheel && (e.shift || e.control))
                {
                    if (e.shift && !e.control)
                    {
                        settings.count += (int)Mathf.Sign(e.delta.y);
                        this.Repaint();
                    }
                    else if (e.control && !e.shift)
                    {
                        float delta = e.delta.y;
                        if (delta % 1 == 0)
                        {
                            settings.radius *= delta > 0f ? 1.1f : 0.9f;
                        }
                        else
                        {
                            settings.radius += delta;
                        }
                        this.Repaint();
                    }
                    else if (e.shift && e.control)
                    {
                        float delta = e.delta.y;
                        if (delta % 1 == 0)
                        {
                            settings.gap += Mathf.Sign(delta) *  0.9f;
                        }
                        else
                        {
                            settings.gap += delta;
                        }
                        this.Repaint();
                    }
                    if (e.type == EventType.ScrollWheel)
                    {
                        e.Use();
                    }
                }

                if (e.type != EventType.KeyDown)
                {
                    return;
                }

                if (e.character == '\t')
                {
                    settings.toolBar++;
                    if (settings.toolBar > settings.menuOptions.Length - 1)
                    {
                        settings.toolBar = 0;
                    }
                    e.Use();
                    this.Repaint();
                }
            }

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

        void HelpInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("Help", new GUIStyle("ProgressBarBack"), GUILayout.Width(Screen.width - 15));
                GUILayout.Space(5);
                GUILayout.Label("[Tab] - Change Up Direction");
                GUILayout.Label("[Ctrl + SrollWheel] - Change Size");
                GUILayout.Label("[Shift + SrollWheel] - Change Object Count");
                GUILayout.Label("[Ctrl + Shift + SrollWheel] - Change Gap");
            }
            EditorGUILayout.EndVertical();
        }

        static LayerMask LayerMaskField(Rect rect, string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != string.Empty)
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }
            maskWithoutEmpty = EditorGUI.MaskField(rect, label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }
            layerMask.value = mask;
            return layerMask;
        }

        public string[] GetSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }
    }
}