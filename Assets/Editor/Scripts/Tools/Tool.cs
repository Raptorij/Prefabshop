using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class BrushKeyCodeAttribute : System.Attribute
    {
        public readonly KeyCode keyCode;

        public BrushKeyCodeAttribute(KeyCode keyCode)
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

        public BrushInfo brushInfo;
        public GameObject targetSpawnObject;

        public float dragDelta;

        private Vector2 previousPosition;

        public List<Parameter> parameters = new List<Parameter>();

        public Tool(BrushInfo info)
        {
            brushInfo = info;
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

        public P GetParameter<P>() where P : Parameter
        {
            return parameters.Find(search => search is P) as P;
        }

        public virtual void SelectTool()
        {
            OnSelectTool?.Invoke();
        }

        public virtual void DeselectTool()
        {
            OnDeselectTool?.Invoke();
        }

        public virtual void CastBrush()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            DrawTool(drawPointRay);
        }

        public virtual void DrawTool(Ray drawPointRay)
        {
            RaycastHit drawPointHit;
            if (Physics.Raycast(drawPointRay, out drawPointHit, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
            {
                DrawHandle(drawPointRay);
                var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                Handles.BeginGUI();
                Handles.Label(mouseRay.origin - Vector3.up * 0.05f + Vector3.forward * 0.05f, $"Name:{drawPointHit.collider.gameObject.name}" +
                                            (drawPointHit.collider.gameObject.transform.parent ? $"\nParent: {drawPointHit.collider.gameObject.transform.parent.name}" : "\nParent: null") +
                                            $"\nTag: {drawPointHit.collider.gameObject.tag}" +
                                            $"\nLayer: {LayerMask.LayerToName(drawPointHit.collider.gameObject.layer)}");
                Handles.EndGUI();

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

        public virtual void DrawHandle(Ray drawPointHit)
        {

        }

        public virtual void Paint(RaycastHit drawPointHit)
        {
            OnPaint?.Invoke(drawPointHit);
        }
    }
}