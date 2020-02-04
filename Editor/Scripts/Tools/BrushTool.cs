using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.B)]
    public class BrushTool : Tool
    {
        public Mesh shape;
        Mesh shapeSide;
        public Texture2D previousTexture;
        public RaycastHit raycastHit;

        public BrushTool() : base()
        {
            var type = this.GetType();

            AddParameter(new InstatiatePrefab(type));
            AddParameter(new PrefabsSet(type));
            AddParameter(new Shape(type));
            AddParameter(new Outer(type));
            AddParameter(new Radius(type));
            AddParameter(new Count(type));
            AddParameter(new Gap(type));
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new Parent(type));
            AddParameter(new Scale(type));
            AddParameter(new Rotation(type));
            AddParameter(new FirstObjectFilter(type));
            AddParameter(new FilterObject(type));
            AddParameter(new IgnoringLayer(type));
            AddParameter(new Mask(type));

            GetParameter<Shape>().onTextureChange += ResetShape;
            GetParameter<Shape>().valueChanged += ResetShape;
            GetParameter<PrefabsSet>().Activate();
        }

        public override void SelectTool()
        {
            base.SelectTool();
        }

        public override void DeselectTool()
        {
            base.DeselectTool();
        }

        protected override void DrawTool(Ray ray)
        {
            base.DrawTool(ray);
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
                    shapeSide = ms.CreateMeshOutline();
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
            var prefabs = GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                var casts = Physics.RaycastAll(castRay, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));

                var originalVert = new Vector3[] { };
                var rotatedVert = new Vector3[] { };

                if (shape != null)
                {
                    var rotation = Quaternion.identity;
                    rotation = Quaternion.LookRotation(raycastHit.normal) * Quaternion.Euler(90f, 0f, 0f);
                    originalVert = new Vector3[shape.vertices.Length];
                    rotatedVert = new Vector3[shape.vertices.Length];
                    originalVert = shape.vertices;

                    for (int i = 0; i < originalVert.Length; i++)
                    {
                        rotatedVert[i] = rotation * originalVert[i];
                    }
                }
                
                for (int k = 0; k < casts.Length; k++)
                {
                    if (CheckCast(casts[k]))
                    {
                        var cast = casts[k];

                        List<RaycastHit> listRaycast = new List<RaycastHit>();

                        var perpendicularX = Vector3.Cross(cast.normal, cast.normal.Y(cast.normal.x + Random.value, cast.normal.z + Random.value)).normalized;
                        var perpendicularY = Vector3.Cross(cast.normal, perpendicularX).normalized;

                        int whileBreaker = GetParameter<Count>().value;
                        do
                        {
                            whileBreaker--;
                            Vector3 randomSeed = Random.insideUnitCircle * GetParameter<Radius>().value;
                            var random = perpendicularX * randomSeed.x + perpendicularY * randomSeed.y;

                            if (GetParameter<Outer>().value)
                            {
                                randomSeed = Outer.RandomPointOnCircleEdge(GetParameter<Radius>().value, Vector3.zero);
                                random = perpendicularX * randomSeed.x + perpendicularY * randomSeed.z;
                            }


                            var position = ( shape == null ? cast.point + random : rotatedVert[Random.Range(0, rotatedVert.Length)] * GetParameter<Radius>().value * 0.075f + cast.point);
                            if (shape != null && GetParameter<Outer>().value)
                            {
                                int idVert = Random.Range(0, shapeSide.vertices.Length);
                                int nextVert = idVert + 1 >= shapeSide.vertices.Length ? 0 : idVert + 1;
                                Vector3 randomPosVerticales = Vector3.Lerp(shapeSide.vertices[idVert], shapeSide.vertices[nextVert], Random.value);
                                position = randomPosVerticales * GetParameter<Radius>().value * 0.075f + cast.point;
                            }
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
                            GetParameter<InstatiatePrefab>().CreateObject(listRaycast[i], this);
                        }
                        break;
                    }
                }
            }
            else
            {
                Debug.Log($"<color=magenta>[Prefabshop] </color> There is no selected any objects in Options");
            }            
        }

        void CreateObject(RaycastHit rayHit)
        {
            var newPos = rayHit.point + rayHit.normal.normalized * GetParameter<Gap>().value;
            var prefabs = GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                var selectedPrefab = prefabs[Random.Range(0, prefabs.Count)];
                GameObject osd = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
                osd.transform.position = newPos;
                if (GetParameter<Scale>().randomScale)
                {
                    osd.transform.localScale *= Random.Range(GetParameter<Scale>().minValue, GetParameter<Scale>().maxValue);
                }
                osd.transform.up = rayHit.normal;
                osd.transform.SetParent(GetParameter<Parent>().value);
                osd.transform.eulerAngles = GetParameter<Rotation>().GetRotation(selectedPrefab);
                osd.tag = GetParameter<Tag>().value;
                osd.layer = GetParameter<Layer>().value;
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
        }

        bool CheckCast(RaycastHit cast)
        {
            var hitObj = cast.collider.gameObject;
            if (!GetParameter<Mask>().CheckPoint(cast.point))
            {
                return false;
            }
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