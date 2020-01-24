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

        public Mesh maskShape;
        public Vector3[] maskOutline;

        float time;

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
            Shortcuts();
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
            DrawMask();
            currentTool?.CastTool();
            Handles.BeginGUI();
            {

                ToolGUIInfo();
                Rect settingsInfoRect = new Rect(1, 1, 35, 30 * possibleTools.Length + 5);
                GUI.Box(settingsInfoRect, "", new GUIStyle("HelpBox"));
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

                if (maskShape != null)
                {
                    float y = 128;
                    Rect maskInfo = new Rect(this.position.xMin + 1, this.position.yMax + y - 1, 128, y);
                    maskInfo = new Rect(1, this.position.yMax - 94 - y, 212, y);
                    GUI.Box(maskInfo, "Mask Options", new GUIStyle("HelpBox"));
                    Rect maskButtonsRect = new Rect(maskInfo.x + 5, maskInfo.y + 5 + 18, 200, 18);
                    if (GUI.Button(maskButtonsRect, "Reset Mask"))
                    {
                        maskShape = null;
                        maskOutline = null;
                        Event.current.Use();
                    }
                    maskButtonsRect.y += 19;
                    GUI.enabled = false;
                    if (GUI.Button(maskButtonsRect, "Select prefabs inside Mask"))
                    {
                        maskShape = null;
                        maskOutline = null;
                        Event.current.Use();
                    }
                    GUI.enabled = true;
                }
            }
            Handles.EndGUI();
            Tools.hidden = blockToggle;
        }

        void ToolGUIInfo()
        {
            var drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit drawPointHit;

            var lablePos = HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition);
            lablePos.y = Screen.height -lablePos.y - 20f;
            lablePos.x += 10f;
            string mouseInfo;
            if (Physics.Raycast(drawPointRay, out drawPointHit, Mathf.Infinity))
            {
                var targetObj = drawPointHit.collider.gameObject;
                mouseInfo =
                    $"Name:{targetObj.name}" +
                    (targetObj.transform.parent ? $"\nParent: {targetObj.transform.parent.name}" : "\nParent: null") +
                    $"\nTag: {targetObj.tag}" +
                    $"\nLayer: {LayerMask.LayerToName(targetObj.layer)}";
            }
            else
            {
                mouseInfo =
                    $"Name:null" +
                    "\nParent: null" +
                    "\nTag: null" +
                    "\nLayer: null";
            }
            var labelRect = GUILayoutUtility.GetRect(new GUIContent(mouseInfo), "label", GUILayout.ExpandWidth(false));
            Rect settingsInfoRect = new Rect(lablePos, labelRect.size);
            GUI.Box(settingsInfoRect, "", new GUIStyle("HelpBox"));
            GUI.Box(settingsInfoRect, "", new GUIStyle("HelpBox"));
            GUI.Label(settingsInfoRect, mouseInfo);
        }

        void DrawMask()
        {
            if (maskShape != null)
            {
                for (int i = 0; i < maskOutline.Length - 1; i++)
                {
                    var colorLine = i % 2 == 0 ? Color.black : Color.white;
                    Handles.color = colorLine;
                    Handles.DrawLine(maskOutline[i], maskOutline[i + 1]);
                }

                var matrix = new Matrix4x4();
                matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
                var mat = new Material(Shader.Find("Raptorij/Lines"));
                mat.SetColor("_Color1", new Color(1, 1, 1, 0.25f));
                mat.SetColor("_Color2", new Color(0, 0, 0, 0.25f));
                mat.SetPass(0);
                Graphics.DrawMeshNow(maskShape, matrix, 0);
            }
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
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            string buttonInfo = " [" + brushKey.ToString() + "] - " + brushType.Name.Replace("Tool", "");
            var labelRect = GUILayoutUtility.GetRect(new GUIContent(buttonInfo), "label", GUILayout.ExpandWidth(false));
            GUI.Box(new Rect(info.position, labelRect.size + Vector2.up * 2.5f), "", new GUIStyle("textField"));
            GUI.Label(new Rect(info.position, labelRect.size), buttonInfo, new GUIStyle("MiniBoldLabel"));
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