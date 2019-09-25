using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.L)]
    public class LineTool : Tool
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public bool isDraw;

        public LineTool(BrushInfo info) : base(info)
        {
            AddParameter(new Count());
            AddParameter(new Gap());
            AddParameter(new Tag());
            AddParameter(new Layer());
            AddParameter(new Parent());
            AddParameter(new Scale());
            AddParameter(new FirstObjectFilter());
            AddParameter(new FilterObject());
            AddParameter(new IgnoringLayer());

            OnStartPaint += StartPaint;
            OnEndPaint += EndPain;
        }

        void StartPaint(RaycastHit drawPointHit)
        {
            startPoint = drawPointHit.point;
            isDraw = true;
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);
        }

        void EndPain(RaycastHit raycastHit)
        {
            isDraw = false;
            float distance = Vector3.Distance(startPoint, endPoint);
            List<Vector3> targetCastsPoint = new List<Vector3>();
            for (int i = 0; i < GetParameter<Count>().value; i++)
            {
                targetCastsPoint.Add(LerpByDistance(startPoint, endPoint, (float)i / GetParameter<Count>().value));
            }

            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            List<RaycastHit> listRaycast = new List<RaycastHit>();

            int whileBreaker = GetParameter<Count>().value * 4;
            for (int i = 0; i < GetParameter<Count>().value; i++)
            {
                whileBreaker--;
                Ray rayRandom = new Ray(castRay.origin, targetCastsPoint[i] - castRay.origin);
                RaycastHit castCheck;

                if (Physics.Raycast(rayRandom, out castCheck, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
                {
                    if (!CheckCast(castCheck))
                    {
                        return;
                    }
                    listRaycast.Add(castCheck);
                }
                if (whileBreaker <= 0)
                {
                    break;
                }
            }

            for (int i = 0; i < listRaycast.Count; i++)
            {
                CreateObject(listRaycast[i]);
            }
        }

        public override void DrawHandle(Ray ray)
        {
            base.DrawHandle(ray);
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            for (int k = 0; k < casts.Length; k++)
            {
                if (CheckCast(casts[k]))
                {
                    endPoint = casts[k].point;
                    break;
                }
            }
            if (casts.Length > 0 && isDraw)
            {
                Handles.color = new Color(0, 1, 0, 1f);
                Handles.DrawLine(startPoint, endPoint);
            }
        }

        bool CheckCast(RaycastHit cast)
        {
            var hitObj = cast.collider.gameObject;
            if (GetParameter<FirstObjectFilter>().value && GetParameter<FirstObjectFilter>().Enable)
            {
                if (targetSpawnObject == hitObj)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (GetParameter<FilterObject>().value != null && GetParameter<FilterObject>().Enable)
            {
                if (GetParameter<FilterObject>().value == hitObj)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
        {
            return (A + x * (B - A));
        }

        void CreateObject(RaycastHit rayHit)
        {
            var newPos = rayHit.point + rayHit.normal.normalized * GetParameter<Gap>().value;

            GameObject osd = PrefabUtility.InstantiatePrefab(brushInfo.brushObjects[Random.Range(0, brushInfo.brushObjects.Count)]) as GameObject;
            osd.transform.position = newPos;
            if (GetParameter<Scale>().randomScale)
            {
                osd.transform.localScale *= Random.Range(GetParameter<Scale>().minValue, GetParameter<Scale>().maxValue);
            }
            //if (paintSettings.randomizeRotation)
            //{
            //    osd.transform.rotation = Random.rotation;
            //}
            osd.transform.up = rayHit.normal;
            //switch (paintSettings.toolBar)
            //{
            //    case 0:
            //        osd.transform.up = Vector3.up;
            //        break;
            //    case 1:
            //        osd.transform.LookAt(SceneView.lastActiveSceneView.camera.transform.position);
            //        break;
            //    case 2:
            //        osd.transform.up = rayHit.normal;
            //        break;
            //}
            osd.transform.SetParent(GetParameter<Parent>().value);
            osd.tag = GetParameter<Tag>().value;
            osd.layer = GetParameter<Layer>().value;
            Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
        }
    }
}