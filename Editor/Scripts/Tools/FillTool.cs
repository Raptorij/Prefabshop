using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.G)]
    public class FillTool : Tool
    {
        public RaycastHit raycastHit;
        GameObject go;
        Material drawMat;

        public FillTool() : base()
        {
            var type = this.GetType();

            AddParameter(new PrefabsSet(type));
            AddParameter(new Count(type));
            AddParameter(new Scale(type));
            AddParameter(new Gap(type));
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new Parent(type));
            AddParameter(new IgnoringLayer(type));
            AddParameter(new ListOfObjects(type));
            AddParameter(new Mask(type));
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
        
        public override void DrawHandle(Ray ray)
        {
            base.DrawHandle(ray);
            if (GetParameter<Mask>().HaveMask)
            {
                RaycastHit drawPointHit;
                if (Physics.Raycast(ray, out drawPointHit, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
                {
                    if (GetParameter<Mask>().CheckPoint(drawPointHit.point))
                    {
                        var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                        mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                        mat.SetPass(0);
                        Graphics.DrawMeshNow(GetParameter<Mask>().MaskShape, Matrix4x4.identity, 0);
                    }
                }
            }
            else
            {
                if (Event.current.type == EventType.MouseMove)
                {
                    go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

                }
                if (go != null)
                {
                    var shape = go.GetComponentInChildren<MeshFilter>().sharedMesh;
                    var position = go.transform.position;
                    var rotation = go.transform.rotation;
                    var scale = go.transform.lossyScale;

                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetTRS(position, rotation, scale);
                    if (drawMat == null)
                    {
                        drawMat = new Material(Shader.Find("Raptorij/BrushShape"));
                    }
                    drawMat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                    drawMat.SetPass(0);
                    Graphics.DrawMeshNow(shape, matrix, 0);
                }
            }
        }

        

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);            
            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (GetParameter<Mask>().HaveMask)
            {
                RaycastHit raycast;
                if (Physics.Raycast(castRay, out raycast, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value)))
                {
                    if (GetParameter<Mask>().CheckPoint(raycast.point))
                    {
                        for (int i = 0; i < GetParameter<Count>().value; i++)
                        {
                            CreateObject(Geometry.GetRandomPointOnMesh(GetParameter<Mask>().MaskShape), Quaternion.identity);
                        }
                    }
                }
            }
            else
            {
                var objectWithMesh = go.GetComponentInChildren<MeshFilter>().transform;
                var prefabs = GetParameter<PrefabsSet>().selectedPrefabs;
                if (prefabs.Count > 0)
                {
                    for (int i = 0; i < GetParameter<Count>().value; i++)
                    {
                        FindPointOnMesh(objectWithMesh);
                    }
                }
                else
                {
                    Debug.Log($"<color=magenta>[Prefabshop] </color> There is no selected any objects in Options");
                }
            }
        }

        void FindPointOnMesh(Transform currentObject)
        {
            var meshPoints = currentObject.GetComponent<MeshFilter>().sharedMesh.vertices;
            int[] tris = currentObject.GetComponent<MeshFilter>().sharedMesh.triangles;
            int triStart = Random.Range(0, meshPoints.Length / 3) * 3;

            float a = Random.value;
            float b = Random.value;

            if (a + b >= 1)
            {
                a = 1 - a;
                b = 1 - b;
            }

            var newPointOnMesh = meshPoints[triStart] + (a * (meshPoints[triStart + 1] - meshPoints[triStart])) + (b * (meshPoints[triStart + 2] - meshPoints[triStart])); // apply formula to get new random point inside triangle

            newPointOnMesh = currentObject.TransformPoint(newPointOnMesh);
            var bounds = GeometryUtility.CalculateBounds(meshPoints, currentObject.localToWorldMatrix);
            
            float r = 0;

            if (bounds.max.x > r)
            {
                r = bounds.max.x;
            }
            if (bounds.max.y > r)
            {
                r = bounds.max.y;
            }
            if (bounds.max.z > r)
            {
                r = bounds.max.z;
            }

            var rayOrigin = ((Random.onUnitSphere * r) + currentObject.position);

            RaycastHit hitPoint = new RaycastHit();
            var casts = Physics.RaycastAll(rayOrigin, newPointOnMesh - rayOrigin, Mathf.Infinity);
            for (int i = 0; i < casts.Length; i++)
            {
                if (casts[i].transform == currentObject)
                {
                    hitPoint = casts[i];
                }
            }
            CreateObject(hitPoint.point, Quaternion.FromToRotation(Vector3.up, hitPoint.normal));
        }

        void CreateObject(Vector3 position, Quaternion rotation)
        {
            var prefabs = GetParameter<PrefabsSet>().selectedPrefabs;
            if (prefabs.Count > 0)
            {
                GameObject osd = PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
                osd.transform.position = position;
                osd.transform.rotation = rotation;
                if (GetParameter<Scale>().randomScale)
                {
                    osd.transform.localScale *= Random.Range(GetParameter<Scale>().minValue, GetParameter<Scale>().maxValue);
                }
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
        }
    }
}
