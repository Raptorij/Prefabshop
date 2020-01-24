using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class ToolKeyCodeAttributeAttribute : System.Attribute
    {
        public readonly KeyCode keyCode;

        public ToolKeyCodeAttributeAttribute(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }
    }

    public abstract class Tool
    {
        public event System.Action OnSelectTool;
        public event System.Action OnDeselectTool;
        public event System.Action<RaycastHit> OnDrawTool;
        public event System.Action<RaycastHit> OnStartPaint;
        public event System.Action<RaycastHit> OnPaint;
        public event System.Action<RaycastHit> OnEndPaint;
        
        public GameObject targetSpawnObject;

        public float dragDelta;

        private Vector2 previousPosition;

        public List<Parameter> parameters = new List<Parameter>();

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
            OnSelectTool?.Invoke();
        }

        public virtual void DeselectTool()
        {
            EditorWindow.GetWindow<Prefabshop>().Repaint();
            EditorWindow.GetWindow<SceneView>().Focus();
            OnDeselectTool?.Invoke();
        }

        public virtual void CastTool()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            DrawTool(drawPointRay);
        }

        protected virtual void DrawTool(Ray drawPointRay)
        {
            RaycastHit drawPointHit;
            if (Physics.Raycast(drawPointRay, out drawPointHit, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
            {
                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        targetSpawnObject = drawPointHit.collider.gameObject;
                        OnStartPaint?.Invoke(drawPointHit);
                    }
                    Paint(drawPointHit);
                }
                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    OnEndPaint?.Invoke(drawPointHit);
                }
                SceneView.RepaintAll();
            }
        }

        public virtual void Paint(RaycastHit drawPointHit)
        {
            OnPaint?.Invoke(drawPointHit);
        }
    }
}