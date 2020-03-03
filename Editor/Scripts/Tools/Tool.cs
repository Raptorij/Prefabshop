using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class ToolKeyCodeAttribute: System.Attribute
    {
        public readonly KeyCode keyCode;

        public ToolKeyCodeAttribute(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }        
    }

    public class ToolColorAttribute : System.Attribute
    {
        public enum ToolUseType
        {
            Paint,
            Remove,
            Other
        }

        public readonly ToolUseType toolUseType;

        public ToolColorAttribute(ToolUseType toolUseType)
        {
            this.toolUseType = toolUseType;
        }

        public static void SaveColor(ToolUseType toolUseType, Color color)
        {
            EditorPrefs.SetString(toolUseType.ToString(), ColorToHex(color));
        }

        public static Color GetColor(ToolUseType toolUseType)
        {
            Color32 defaultColor = new Color32(255,255,255,255);
            switch (toolUseType)
            {
                case ToolUseType.Paint:
                    defaultColor = new Color32(0, 255, 0, 64);
                    break;
                case ToolUseType.Remove:
                    defaultColor = new Color32(255, 0, 0, 64);
                    break;
                case ToolUseType.Other:
                    defaultColor = new Color32(255, 255, 0, 64);
                    break;
            }
            string hexDefault = defaultColor.r.ToString("X2") + 
                                defaultColor.g.ToString("X2") + 
                                defaultColor.b.ToString("X2") + 
                                defaultColor.a.ToString("X2");

            return HexToColor(EditorPrefs.GetString(toolUseType.ToString(), hexDefault));
        }

        static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
            return hex;
        }

        static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }
    }

    public abstract class Tool
    {
        [MenuItem(itemName: "Assets/Create/Prefabshop/New Tool Script", isValidateFunction: false, priority: 51)]
        public static void CreateScriptFromTemplate()
        {
            var path = AssetDatabase.GetAssetPath(Resources.Load("Tool.cs"));
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewTool.cs");
        }

        public event System.Action OnSelectTool;
        public event System.Action OnDeselectTool;
        public event System.Action<RaycastHit> OnDrawTool;

        public string info;
        public Color toolColor;
        public List<Parameter> parameters = new List<Parameter>();
        ToolColorAttribute attribute;

        public Tool()
        {

        }

        protected Parameter AddParameter(Parameter parameter)
        {
            if (!parameters.Exists(search => search.GetType() ==  parameter.GetType()))
            {
                parameters.Add(parameter);
                return parameter;
            }
            return null;
        }

        protected Parameter AddParameter(Parameter parameter, int id)
        {
            if (!parameters.Exists(search => search.Identifier == parameter.Identifier))
            {
                parameters.Add(parameter);
                return parameter;
            }
            return null;
        }

        public P GetParameter<P>() where P : Parameter
        {
            return parameters.Find(search => search is P) as P;
        }

        public P GetParameter<P>(int id) where P : Parameter
        {
            return parameters.Find(search => search is P && search.Identifier == id) as P;
        }

        public virtual void SelectTool()
        {
            attribute = this.GetType().GetCustomAttribute(typeof(ToolColorAttribute)) as ToolColorAttribute;
            OnSelectTool?.Invoke();
        }

        public virtual void DeselectTool()
        {
            EditorWindow.GetWindow<Prefabshop>().Repaint();
            EditorWindow.GetWindow<SceneView>().Focus();
            OnDeselectTool?.Invoke();
        }

        public virtual void OnGUI()
        {
        }

        public virtual void CastTool()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            DrawTool(drawPointRay);
        }

        protected virtual void DrawTool(Ray drawPointRay)
        {
            if (attribute != null)
            {
                toolColor = ToolColorAttribute.GetColor(attribute.toolUseType);
            }
            RaycastHit drawPointHit;
            if (Physics.Raycast(drawPointRay, out drawPointHit, Mathf.Infinity, ~(GetParameter<Ignore>().layer)))
            {
                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        OnStartPaint(drawPointHit);
                    }
                    Paint(drawPointHit);
                }
                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    OnEndPaint(drawPointHit);
                }
            }
            OnDrawTool?.Invoke(drawPointHit);
        }

        protected virtual void OnStartPaint(RaycastHit startPointHit)
        {

        }

        public virtual void Paint(RaycastHit drawPointHit)
        {

        }

        protected virtual void OnEndPaint(RaycastHit endPointHit)
        {

        }
    }
}