using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.B)]
    public class PaintBrush : Brush
    {
        public PaintBrush(BrushInfo into, PaintSettings settings) : base(into, settings) { }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var mp = Event.current.mousePosition;
            var castRay = HandleUtility.GUIPointToWorldRay(mp);
            RaycastHit cast;
            if (Physics.Raycast(castRay, out cast, Mathf.Infinity, ~(paintSettings.ignoringLayer)))
            {
                List<RaycastHit> listRaycast = new List<RaycastHit>();

                var perpendicularX = Vector3.Cross(cast.normal, cast.normal.Y(cast.normal.x + Random.value, cast.normal.z + Random.value)).normalized;
                var perpendicularY = Vector3.Cross(cast.normal, perpendicularX).normalized;

                int whileBreaker = paintSettings.count * 4;
                do
                {
                    whileBreaker--;
                    var randomSeed = Random.insideUnitCircle * paintSettings.radius;
                    var random = perpendicularX * randomSeed.x + perpendicularY * randomSeed.y;

                    Ray rayRandom = new Ray(castRay.origin, cast.point + random - castRay.origin);
                    RaycastHit castCheck;

                    if (Physics.Raycast(rayRandom, out castCheck, Mathf.Infinity, ~(paintSettings.ignoringLayer)))
                    {
                        var hitObj = castCheck.collider.gameObject;
                        if (paintSettings.firstObjectFilter)
                        {
                            if (targetSpawnObject == hitObj)
                            {
                                listRaycast.Add(castCheck);
                            }
                        }
                        else if (paintSettings.filterObject != null)
                        {
                            if (paintSettings.filterObject == hitObj)
                            {
                                listRaycast.Add(castCheck);
                            }
                        }
                        else
                        {
                            listRaycast.Add(castCheck);
                        }
                    }
                    if (whileBreaker <= 0)
                    {
                        break;
                    }
                } while (listRaycast.Count < paintSettings.count);

                for (int i = 0; i < listRaycast.Count; i++)
                {
                    CreateObject(listRaycast[i], paintSettings.radius);
                }
            }
        }

        void CreateObject(RaycastHit rayHit, float radius)
        {
            var newPos = rayHit.point + rayHit.normal.normalized * paintSettings.gap;

            GameObject osd = PrefabUtility.InstantiatePrefab(brushInfo.brushObjects[Random.Range(0, brushInfo.brushObjects.Count)]) as GameObject;
            osd.transform.position = newPos;
            if (paintSettings.randomizeScale)
            {
                osd.transform.localScale *= Random.Range(paintSettings.randomScaleMin, paintSettings.randomScaleMax);
            }
            if (paintSettings.randomizeRotation)
            {
                osd.transform.rotation = Random.rotation;
            }
            osd.transform.up = rayHit.normal;
            switch (paintSettings.toolBar)
            {
                case 0:
                    osd.transform.up = Vector3.up;
                    break;
                case 1:
                    osd.transform.LookAt(SceneView.lastActiveSceneView.camera.transform.position);
                    break;
                case 2:
                    osd.transform.up = rayHit.normal;
                    break;
            }


            osd.transform.SetParent(paintSettings.targetParent);
            osd.tag = paintSettings.targetTag;
            osd.layer = paintSettings.targetLayer;
            Undo.RegisterCreatedObjectUndo(osd, "Create Prefab");
        }
    }
}