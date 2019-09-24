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

    public abstract class Brush
    {
        public event System.Action<RaycastHit> OnDrawTool;
        public event System.Action OnStartPaint;
        public event System.Action OnPaint;
        public event System.Action OnEndPaint;

        public BrushInfo brushInfo;
        public PaintSettings paintSettings;
        public GameObject targetSpawnObject;

        public float dragDelta;

        private Vector2 previousPosition;

        public List<Parameter> parameters = new List<Parameter>();

        public Brush(BrushInfo info, PaintSettings settings)
        {
            brushInfo = info;
            paintSettings = info == null ? settings : brushInfo.settings;
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
                DrawHandle(drawPointHit);
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
                    }
                    Paint(drawPointHit);
                }
                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    OnEndPaint?.Invoke();
                }
                SceneView.RepaintAll();
            }
        }

        public virtual void DrawHandle(RaycastHit drawPointHit)
        {

        }

        public virtual void Paint(RaycastHit drawPointHit)
        {

        }
    }
}