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
                            bi.brushObjects = brushObjects;
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
                            brushObjects = brushInfoCurrent.brushObjects;
                            if (currentBrush != null)
                            {
                                var brushType = currentBrush.GetType();
                                var constructor = brushType.GetConstructor(new System.Type[] { typeof(BrushInfo), typeof(PaintSettings) });
                                currentBrush = constructor.Invoke(new object[] { brushInfoCurrent, paintSettings }) as Brush;
                            }
                        }
                    }
                    GUI.enabled = brushInfoCurrent != null;
                    if (GUILayout.Button("Clear Current Brush"))
                    {
                        brushObjects.Clear();
                    }
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
                Rect settingsInfoRect = new Rect(5, 5, 480, EditorGUIUtility.singleLineHeight * 8);

                GUI.Box(settingsInfoRect, "", new GUIStyle("HelpBox"));
                BrushSettingsView(settingsInfoRect, currentSettings);

                Rect buttonRect = new Rect(5, settingsInfoRect.y + settingsInfoRect.height + 5, 25, 25);

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
            var filedsRect = windowRect;
            filedsRect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(filedsRect, "Paint Settings" + (brushInfoCurrent != null ? ": " + brushInfoCurrent.name : ""), new GUIStyle("HelpBox"));
            filedsRect.width /= 2f;
            filedsRect.x += 5;
            filedsRect.y += 5;

            EditorGUIUtility.labelWidth = 85;
            filedsRect.y += EditorGUIUtility.singleLineHeight;
            settings.size = EditorGUI.DelayedFloatField(filedsRect, "Size:", settings.size);
            settings.size = settings.size >= 0 ? settings.size : 0;

            filedsRect.y += EditorGUIUtility.singleLineHeight;
            settings.count = EditorGUI.DelayedIntField(filedsRect, "Object Count:", settings.count);
            settings.count = settings.count >= 1 ? settings.count : 1;

            filedsRect.y += EditorGUIUtility.singleLineHeight;
            settings.gap = EditorGUI.DelayedFloatField(filedsRect, "Gap:", settings.gap);

            filedsRect.y += EditorGUIUtility.singleLineHeight;
            filedsRect.width /= 2;
            settings.randomScaleMin = EditorGUI.FloatField(filedsRect, "Scale Min:", settings.randomScaleMin);
            settings.randomScaleMin = settings.randomScaleMin >= 0 ? settings.randomScaleMin : 0;

            filedsRect.x += 125;
            EditorGUIUtility.labelWidth = 70;
            filedsRect.width -= 5;
            settings.randomScaleMax = EditorGUI.FloatField(filedsRect, "Max:", settings.randomScaleMax);
            settings.randomScaleMax = settings.randomScaleMax >= 0 ? settings.randomScaleMax : 0;

            settings.randomScaleMin = settings.randomScaleMin > settings.randomScaleMax ? settings.randomScaleMax : settings.randomScaleMin;

            EditorGUIUtility.labelWidth = 85;
            filedsRect.x -= 125;
            filedsRect.width += 5;
            filedsRect.y += EditorGUIUtility.singleLineHeight;


            settings.checkObjeck = EditorGUI.Toggle(filedsRect, "Check Object:", settings.checkObjeck);

            filedsRect.y += EditorGUIUtility.singleLineHeight;
            GUI.Label(filedsRect, "Up Direction:");
            filedsRect.x += 80;
            filedsRect.width += 45;
            settings.toolBar = GUI.Toolbar(filedsRect, settings.toolBar, settings.menuOptions);

            filedsRect.width = 225;
            filedsRect.x += 245 - 80;
            filedsRect.y -= EditorGUIUtility.singleLineHeight * 5;

            settings.targetParent = EditorGUI.ObjectField(filedsRect, "Parent:", settings.targetParent, typeof(Transform), true) as Transform;

            filedsRect.y += EditorGUIUtility.singleLineHeight + 1;
            settings.targetTag = EditorGUI.TagField(filedsRect, "Tag:", settings.targetTag);

            filedsRect.y += EditorGUIUtility.singleLineHeight + 1;
            settings.targetLayer = EditorGUI.LayerField(filedsRect, "Layer:", settings.targetLayer);

            filedsRect.y += EditorGUIUtility.singleLineHeight + 1;
            settings.ignoringLayer = LayerMaskField(filedsRect, "Ignored Layers:", settings.ignoringLayer);
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
                        settings.size += (int)Mathf.Sign(e.delta.y);
                        this.Repaint();
                    }
                    else if (e.shift && e.control)
                    {
                        settings.gap += Time.unscaledDeltaTime * (int)Mathf.Sign(e.delta.y) * 10f;
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
                    Tools.hidden = blockToggle;
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