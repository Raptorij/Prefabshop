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
        public BrushInfo brushInfo;
        public PaintSettings paintSettings;
        public GameObject targetSpawnObject;

        public float dragDelta;

        private Vector2 previousPosition;

        public Brush(BrushInfo info, PaintSettings settings)
        {
            brushInfo = info;
            paintSettings = info == null ? settings : brushInfo.settings;
        }

        public virtual void CastBrush()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit drawPointHit;

            if (Physics.Raycast(drawPointRay, out drawPointHit, Mathf.Infinity, ~(paintSettings.ignoringLayer)))
            {
                Handles.color = paintSettings.placeBrush;
                Handles.DrawSolidDisc(drawPointHit.point, drawPointHit.normal, paintSettings.radius);
                Handles.color = Color.white;
                Handles.DrawWireDisc(drawPointHit.point, drawPointHit.normal, paintSettings.radius);
                Handles.color = Color.yellow;
                Handles.DrawLine(drawPointHit.point, drawPointHit.point + drawPointHit.normal * paintSettings.gap);

                Handles.color = Color.white;

                Vector3 mousePosition = Event.current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                mousePosition = ray.origin;

                Handles.BeginGUI();
                Handles.Label(mousePosition - Vector3.up * 0.05f + Vector3.forward * 0.05f, $"Name:{drawPointHit.collider.gameObject.name}" +
                                            (drawPointHit.collider.gameObject.transform.parent ?
                                            $"\nParent: {drawPointHit.collider.gameObject.transform.parent.name}" : "\nParent: null") +
                                            $"\nTag: {drawPointHit.collider.gameObject.tag}" +
                                            $"\nLayer: {LayerMask.LayerToName(drawPointHit.collider.gameObject.layer)}");
                Handles.EndGUI();
                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        targetSpawnObject = drawPointHit.collider.gameObject;
                        previousPosition = mousePosition;
                    }
                    else
                    {
                        dragDelta = Vector2.Distance(mousePosition, previousPosition) * paintSettings.radius * 12.5f;
                        previousPosition = mousePosition;
                        if (dragDelta < 1)
                        {
                            return;
                        }
                    }
                    Paint(drawPointHit);
                }
                SceneView.RepaintAll();
            }
        }

        public virtual void Paint(RaycastHit drawPointHit)
        {

        }
    }
}