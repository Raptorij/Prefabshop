using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.L)]
    public class PolygonalLassoTool : Tool
    {
        public Mesh shapeSelection;
        Mesh squareMesh;

        public List<Vector3> selectionPoints = new List<Vector3>();

        public PolygonalLassoTool() : base()
        {
            var type = this.GetType();
            AddParameter(new Count(type));
            AddParameter(new FirstObjectFilter(type));
            AddParameter(new FilterObject(type));
            AddParameter(new IgnoringLayer(type));
        }

        public override void SelectTool()
        {
            base.SelectTool();
        }

        public override void DeselectTool()
        {
            base.DeselectTool();
        }

        public override void DrawTool(Ray drawPointHit)
        {
            base.DrawTool(drawPointHit);
            for (int i = 0; i < selectionPoints.Count - 1; i++)
            {
                var colorLine = i % 2 == 0 ? Color.yellow : Color.red;
                Handles.color = colorLine;
                Handles.DrawLine(selectionPoints[i], selectionPoints[i + 1]);
            }

            if (squareMesh != null)
            {
                var matrix = new Matrix4x4();
                matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
                var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                mat.SetColor("_Color", new Color(1, 1, 1, 0.125f));
                mat.SetPass(0);
                Graphics.DrawMeshNow(squareMesh, matrix, 0);

                Vector3 centerOfMask = Vector3.zero;
                var squareVert = squareMesh.vertices;
                centerOfMask.x = (squareVert[0].x + squareVert[1].x + squareVert[2].x + squareVert[3].x) / 4;
                //m.y = (p1.y + p2.y + p3.y + p4.y) / 4;
                centerOfMask.z = (squareVert[0].z + squareVert[1].z + squareVert[2].z + squareVert[3].z) / 4;

                Handles.DrawSphere(0, centerOfMask, Quaternion.identity, 1f);
            }

            if (shapeSelection != null)
            {
                var matrix = new Matrix4x4();
                matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
                var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                mat.SetColor("_Color", new Color(1, 0, 0, 0.5f));
                mat.SetPass(0);
                Graphics.DrawMeshNow(shapeSelection, matrix, 0);
            }
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (e.control)
                {
                    selectionPoints.Add(selectionPoints[0]);
                    CreateSuqareOverMask();
                    selectionPoints.Add(selectionPoints[0]);
                    float z = squareMesh.vertices[3].z - squareMesh.vertices[0].z;
                    float x = squareMesh.vertices[3].x - squareMesh.vertices[0].x;

                    float acepRatioX = (x / z);
                    int acepRatioZ = (int)(z / x);

                    int count = GetParameter<Count>().value;

                    int[,] textureMap = new int[count, count];

                    for (int i = 0; i < count; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {

                            float X = squareMesh.vertices[0].x + x * ((float)j / count);
                            float Z = squareMesh.vertices[0].z + z * ((float)i / count);

                            Vector3 point = new Vector3(x * ((float)j / count), 0, z * ((float)i / count));
                            textureMap[j, i] = Geometry.PointInPolygon(X, Z, selectionPoints.ToArray()) ? 1 : 0;
                            if (Geometry.PointInPolygon(X, Z, selectionPoints.ToArray()))
                            {
                                Debug.Log("point is inside :: " + point);
                            }
                        }
                    }

                    if (selectionPoints.Count >= 3)
                    {
                        MarchingSquares ms = new MarchingSquares();
                        shapeSelection = ms.GenerateMesh(textureMap, 1f);

                        var originalVerts = new Vector3[shapeSelection.vertices.Length];
                        originalVerts = shapeSelection.vertices;

                        var rotatedVerts = new Vector3[originalVerts.Length];
                        var qAngle = Quaternion.AngleAxis(-90, Vector3.up);

                        Vector3 centerOfMask = Vector3.zero;
                        var squareVert = squareMesh.vertices;
                        centerOfMask.x = (squareVert[0].x + squareVert[1].x + squareVert[2].x + squareVert[3].x) / 4;
                        //m.y = (p1.y + p2.y + p3.y + p4.y) / 4;
                        centerOfMask.z = (squareVert[0].z + squareVert[1].z + squareVert[2].z + squareVert[3].z) / 4;

                        for (int i = 0; i < originalVerts.Length; i++)
                        {
                            var vertex = originalVerts[i];
                            vertex.x = vertex.x * acepRatioX;
                            vertex.y = vertex.y * 1;
                            vertex.z = vertex.z * 1;
                            originalVerts[i] = vertex;
                            originalVerts[i] += centerOfMask;
                            //rotatedVerts[i] = qAngle * originalVerts[i];
                        }

                        shapeSelection.vertices = originalVerts;
                        //shapeSide = ms.CreateMeshOutline();
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                    }
                }
                else if (e.shift)
                {
                    selectionPoints.Add(drawPointHit.point);
                }
                else
                {
                    selectionPoints.Clear();
                    selectionPoints.Add(drawPointHit.point);
                }
                return;
            }
        }

        void CreateSuqareOverMask()
        {
            squareMesh = new Mesh();

            selectionPoints.RemoveAt(selectionPoints.Count - 1);
            var vertices = Geometry.GetSquareOverPoints(selectionPoints.ToArray());

            squareMesh.vertices = vertices;

            var tris = new int[6]{ 0, 2, 1, 2, 3, 1};
            squareMesh.triangles = tris;

            var normals = new Vector3[4]{Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            squareMesh.normals = normals;
        }
    }
}