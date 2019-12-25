using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.L)]
    public class LineTool : Tool
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 startPointHandle;
        public Vector3 endPointHandle;
        public bool isDraw;

        public LineTool() : base()
        {
            var type = this.GetType();

            AddParameter(new PrefabsSet(type));
            AddParameter(new Count(type));
            AddParameter(new Gap(type));
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new Parent(type));
            AddParameter(new Scale(type));
            AddParameter(new FirstObjectFilter(type));
            AddParameter(new FilterObject(type));
            AddParameter(new IgnoringLayer(type));
            AddParameter(new Mask(type));

            OnStartPaint += StartPaint;
            OnEndPaint += EndPain;
            GetParameter<PrefabsSet>().Activate();
        }

        void StartPaint(RaycastHit drawPointHit)
        {
            startPointHandle = drawPointHit.point;
            startPoint = Camera.current.WorldToScreenPoint(drawPointHit.point);
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

            List<RaycastHit> listRaycast = new List<RaycastHit>();

            int whileBreaker = GetParameter<Count>().value * 4;
            var currentCamera = Camera.current;
            for (int i = 0; i < GetParameter<Count>().value; i++)
            {
                var castRay = currentCamera.ScreenPointToRay(targetCastsPoint[i]);
                whileBreaker--;
                RaycastHit castCheck;
                
                if (Physics.Raycast(castRay, out castCheck, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
                {
                    if (!GetParameter<Mask>().CheckPoint(castCheck.point))
                    {
                        continue;
                    }
                    if (!CheckCast(castCheck))
                    {
                        continue;
                    }
                    listRaycast.Add(castCheck);
                }
                if (whileBreaker <= 0)
                {
                    break;
                }
            }

            var prefabs = GetParameter<PrefabsSet>().selectedPrefabs;
            if (prefabs.Count > 0)
            {
                for (int i = 0; i < listRaycast.Count; i++)
                {
                    CreateObject(listRaycast[i]);
                }
            }
            else
            {
                Debug.Log($"<color=magenta>[Prefabshop] </color> There is no selected any objects in Options");
            }
        }

        public override void DrawHandle(Ray ray)
        {
            base.DrawHandle(ray);
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            var closest = Mathf.Infinity;
            var currentCamera = Camera.current;
            for (int k = 0; k < casts.Length; k++)
            {
                if (casts[k].distance < closest && CheckCast(casts[k]))
                {
                    closest = casts[k].distance;
                    endPointHandle = casts[k].point;
                    endPoint = Camera.current.WorldToScreenPoint(casts[k].point);
                }
            }
            if (casts.Length > 0 && isDraw)
            {
                Handles.color = new Color(0, 1, 0, 1f);
                Handles.DrawLine(startPointHandle, endPointHandle);
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
            var prefabs = GetParameter<PrefabsSet>().selectedPrefabs;
            if (prefabs.Count > 0)
            {
                GameObject osd = PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
                osd.transform.position = newPos;
                if (GetParameter<Scale>().randomScale)
                {
                    osd.transform.localScale *= Random.Range(GetParameter<Scale>().minValue, GetParameter<Scale>().maxValue);
                }
                osd.transform.up = rayHit.normal;
                osd.transform.SetParent(GetParameter<Parent>().value);
                osd.tag = GetParameter<Tag>().value;
                osd.layer = GetParameter<Layer>().value;
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
        }
    }
}