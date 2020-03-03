using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolColor(ToolColorAttribute.ToolUseType.Paint)]
    [ToolKeyCodeAttribute(KeyCode.L)]
    public class LineTool : Tool
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 startPointHandle;
        public Vector3 endPointHandle;
        public bool isDraw;
        private GameObject targetSpawnObject;

        public LineTool() : base()
        {
            var type = this.GetType();

            AddParameter(new InstatiatePrefab(type));
            AddParameter(new PrefabsSet(type));
            AddParameter(new SelectToolBar(type));
            AddParameter(new Count(type));
            AddParameter(new Step(type));
            AddParameter(new Gap(type));
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new Parent(type));
            AddParameter(new Scale(type));
            AddParameter(new Rotation(type));
            AddParameter(new FirstObjectFilter(type));
            AddParameter(new FilterObject(type));
            AddParameter(new Ignore(type));
            AddParameter(new Mask(type));
            GetParameter<SelectToolBar>().toolBar = new string[] { "By Count", "By Step", "Both" };
            GetParameter<PrefabsSet>().Activate();
        }

        protected override void OnStartPaint(RaycastHit startPointHit)
        {
            base.OnStartPaint(startPointHit);
            targetSpawnObject = startPointHit.collider.gameObject;
            startPointHandle = startPointHit.point;
            startPoint = Camera.current.WorldToScreenPoint(startPointHit.point);
            isDraw = true;
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);
        }

        protected override void OnEndPaint(RaycastHit endPointHit)
        {         
            isDraw = false;
            int idToolBar = GetParameter<SelectToolBar>().idSelect;
            switch (idToolBar)
            {
                case 0:
                    CalculateByCount();
                    break;
                case 1:
                    CalculateByStep();
                    break;
                case 2:
                    break;
            }
            info = $"\nDistance: {0}";
        }

        void CalculateByStep()
        {
            float distance = Vector3.Distance(startPointHandle, endPointHandle);
            List<Vector3> selectedPositions = new List<Vector3>();
            selectedPositions.Add(startPointHandle);
            var prevPos = startPointHandle;
            for (float i = 0; i < distance;)
            {
                var targetPos = GetParameter<Step>().GetSnappedPosition(prevPos, endPointHandle);
                float dist = (targetPos - prevPos).magnitude;
                i += dist;
                if (i <= distance)
                {
                    Vector3 from = targetPos + Vector3.up * 100f;
                    RaycastHit hit;
                    Physics.Raycast(from, Vector3.down, out hit, Mathf.Infinity);
                    selectedPositions.Add(hit.point);
                    prevPos = targetPos;
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i < selectedPositions.Count; i++)
            {
                var prefabs = GetParameter<PrefabsSet>().selectedPrefabs;
                if (prefabs.Count > 0)
                {
                    GetParameter<InstatiatePrefab>().CreateObject(selectedPositions[i], this);
                }
            }
        }

        void CalculateByCount()
        {
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

                if (Physics.Raycast(castRay, out castCheck, Mathf.Infinity, ~(GetParameter<Ignore>().layer)))
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
                    GetParameter<InstatiatePrefab>().CreateObject(listRaycast[i], this);
                }
            }
            else
            {
                Debug.Log($"<color=magenta>[Prefabshop] </color> There is no selected any objects in Options");
            }
        }

        protected override void DrawTool(Ray ray)
        {
            base.DrawTool(ray);
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<Ignore>().layer));
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
                Handles.color = toolColor;
                Handles.DrawLine(startPointHandle, endPointHandle);

                float distance = Vector3.Distance(startPointHandle, endPointHandle);
                info = $"\nDistance: {distance}";
                List<Vector3> selectedPositions = new List<Vector3>();
                selectedPositions.Add(startPointHandle);
                var prevPos = startPointHandle;
                for (float i = 0; i < distance;)
                {
                    var targetPos = GetParameter<Step>().GetSnappedPosition(prevPos, endPointHandle);
                    float dist = (targetPos - prevPos).magnitude;
                    i += dist;
                    if (i <= distance)
                    {
                        Vector3 from = targetPos + Vector3.up * 100f;
                        RaycastHit hit;
                        Physics.Raycast(from, Vector3.down, out hit, Mathf.Infinity);
                        selectedPositions.Add(hit.point);
                        prevPos = targetPos;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 0; i < selectedPositions.Count; i++)
                {
                    var pos = selectedPositions[i];
                    Handles.DrawLine(pos, pos + Vector3.up);
                    Handles.CubeHandleCap(0, pos + Vector3.up, Quaternion.LookRotation(Vector3.up + Vector3.forward), .5f, EventType.Repaint);
                }
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
    }
}