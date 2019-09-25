using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.B)]
    public class BrushTool : Tool
    {      
        public BrushTool(BrushInfo into) : base(into)
        {
            AddParameter(new Radius());
            AddParameter(new Count());
            AddParameter(new Gap());
            AddParameter(new Tag());
            AddParameter(new Layer());
            AddParameter(new Parent());
            AddParameter(new Scale());
            AddParameter(new FirstObjectFilter());
            AddParameter(new FilterObject());
            AddParameter(new IgnoringLayer());
            //AddParameter(new IgnoreSpawnedPrefabs());
        }

        public override void DrawHandle(Ray ray)
        {
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            var raycastHit = casts[0];
            for (int k = 0; k < casts.Length; k++)
            {
                if (CheckCast(casts[k]))
                {
                    raycastHit = casts[k];
                    break;
                }
            }
            Handles.color = new Color(0, 1, 0, 0.1f);
            Handles.DrawSolidDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
            Handles.color = Color.white;
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
            Handles.color = Color.yellow;
            Handles.DrawLine(raycastHit.point, raycastHit.point + raycastHit.normal * GetParameter<Gap>().value);
            Handles.color = Color.white;
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);
            
            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var casts = Physics.RaycastAll(castRay, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));

            for (int k = 0; k < casts.Length; k++)
            {
                if (CheckCast(casts[k]))
                {
                    var cast = casts[k];

                    List<RaycastHit> listRaycast = new List<RaycastHit>();

                    var perpendicularX = Vector3.Cross(cast.normal, cast.normal.Y(cast.normal.x + Random.value, cast.normal.z + Random.value)).normalized;
                    var perpendicularY = Vector3.Cross(cast.normal, perpendicularX).normalized;

                    int whileBreaker = GetParameter<Count>().value * 4;
                    do
                    {
                        whileBreaker--;
                        var randomSeed = Random.insideUnitCircle * GetParameter<Radius>().value;
                        var random = perpendicularX * randomSeed.x + perpendicularY * randomSeed.y;

                        Ray rayRandom = new Ray(castRay.origin, cast.point + random - castRay.origin);
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
                    } while (listRaycast.Count < GetParameter<Count>().value);

                    for (int i = 0; i < listRaycast.Count; i++)
                    {
                        CreateObject(listRaycast[i]);
                    }
                    break;
                }
            }
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
    }    
}