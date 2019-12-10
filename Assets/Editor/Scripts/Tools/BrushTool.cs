using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.B)]
    public class BrushTool : Tool
    {
        public Mesh shape;
        public Texture2D previousTexture;
        public RaycastHit raycastHit;

        public BrushTool(BrushInfo into) : base(into)
        {
            AddParameter(new Shape());
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

            GetParameter<Shape>().OnTextureChange += ResetShape;
            GetParameter<Shape>().OnValueChange += ResetShape;
        }

        public override void SelectTool()
        {
            base.SelectTool();
        }

        public override void DeselectTool()
        {
            base.DeselectTool();
        }

        public override void DrawHandle(Ray ray)
        {
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            var closest = Mathf.Infinity;
            for (int k = 0; k < casts.Length; k++)
            {
                var cast = casts[k];
                if (cast.distance < closest)
                {
                    closest = cast.distance;
                    raycastHit = cast;
                }
            }
            if (GetParameter<Shape>().Texture != null)
            {
                if (shape == null || previousTexture != GetParameter<Shape>().Texture)
                {
                    var t = GetParameter<Shape>().Texture;
                    previousTexture = t;
                    var textureMap = new int[t.width, t.height];

                    int invert = GetParameter<Shape>().Invert ? 0 : 1;

                    var pixels = t.GetPixels();

                    for (int i = 0; i < textureMap.GetLength(0); i++)
                    {
                        for (int j = 0; j < textureMap.GetLength(1); j++)
                        {
                            textureMap[i, j] = pixels[i + j * t.width].r >= .6f ? invert : 1 - invert;
                        }
                    }
                    MarchingSquares ms = new MarchingSquares();
                    shape = ms.GenerateMesh(textureMap, 0.01f * GetParameter<Radius>().value);
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
                else
                {
                    DrawShape();
                }
            }
            else
            {
                Handles.color = new Color(0, 1, 0, 0.1f);
                Handles.DrawSolidDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
                Handles.color = Color.white;
                Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
                Handles.color = Color.yellow;
                Handles.DrawLine(raycastHit.point, raycastHit.point + raycastHit.normal * GetParameter<Gap>().value);
                Handles.color = Color.white;
            }
        }

        void DrawShape()
        {
            if (shape != null)
            {
                var position = raycastHit.point;
                var rotation = Quaternion.identity;
                rotation = Quaternion.LookRotation(raycastHit.normal) * Quaternion.Euler(90f, 0f, 0f);
                var scale = Vector3.one * GetParameter<Radius>().value * 0.075f;


                var matrix = new Matrix4x4();
                matrix.SetTRS(position, rotation, scale);
                var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                mat.SetPass(0);
                Graphics.DrawMeshNow(shape, matrix, 0);
            }
        }

        void ResetShape()
        {
            shape = null;
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

                        var position = shape == null ? cast.point + random : shape.vertices[Random.Range(0, shape.vertices.Length)] * GetParameter<Radius>().value * 0.05f + cast.point;

                        Ray rayRandom = new Ray(castRay.origin, position - castRay.origin);
                        RaycastHit castCheck;

                        if (Physics.Raycast(rayRandom, out castCheck, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
                        {
                            if (!CheckCast(castCheck))
                            {
                                break;
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