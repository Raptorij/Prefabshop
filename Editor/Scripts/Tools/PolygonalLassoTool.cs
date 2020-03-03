using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.P)]
    public class PolygonalLassoTool : Tool
    {
        public Mesh shapeSelection;
        Mesh squareMesh;
        bool isPaint;

        public List<Vector3> selectionPoints = new List<Vector3>();

        public PolygonalLassoTool() : base()
        {
            var type = this.GetType();
            AddParameter(new Count(type));
            AddParameter(new FirstObjectFilter(type));
            AddParameter(new FilterObject(type));
            AddParameter(new Ignore(type));

            var previousFocus = EditorWindow.focusedWindow;
            var prefabshop = EditorWindow.GetWindow<Prefabshop>();
            prefabshop.onMaskReset += OnMaskReset;
            previousFocus.Focus();
        }

        private void OnMaskReset()
        {
            shapeSelection = null;
            squareMesh = null;
            selectionPoints.Clear();
        }

        public override void SelectTool()
        {
            base.SelectTool();
            selectionPoints = new List<Vector3>();
        }

        public override void DeselectTool()
        {
            base.DeselectTool();
            selectionPoints = new List<Vector3>();
        }

        protected override void DrawTool(Ray drawPointHit)
        {
            base.DrawTool(drawPointHit);
            for (int i = 0; i < selectionPoints.Count - 1; i++)
            {
                var colorLine = i % 2 == 0 ? Color.black : Color.white;
                Handles.color = colorLine;
                Handles.DrawLine(selectionPoints[i], selectionPoints[i + 1]);
            }

            var e = Event.current;
            if (e.button == 0)
            {
                if (e.type == EventType.MouseUp && !e.shift && !e.control)
                {
                    GenerateMaskMap();
                }
            }
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var e = Event.current;

            isPaint = e.button == 0 && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown);
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                selectionPoints.Add(drawPointHit.point);
                e.Use();
                return;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (e.control)
                {
                    GenerateMaskMap();
                }
                else if (e.shift)
                {
                    selectionPoints.Add(drawPointHit.point);
                }
                else
                {
                    var previousFocus = EditorWindow.focusedWindow;
                    EditorWindow.GetWindow<Prefabshop>().maskShape = null;
                    previousFocus.Focus();
                    selectionPoints.Clear();
                    selectionPoints.Add(drawPointHit.point);
                }
                e.Use();
                return;
            }
        }

        void GenerateMaskMap()
        {
            selectionPoints.Add(selectionPoints[0]);
            CreateSuqareOverMask();
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
                }
            }

            if (selectionPoints.Count >= 3)
            {
                CreateMaskMesh();
            }
        }

        void CreateMaskMesh()
        {
            var points2d = new Vector2[selectionPoints.Count];
            for (int i = 0; i < points2d.Length; i++)
            {
                points2d[i] = new Vector2(selectionPoints[i].x, selectionPoints[i].z);
            }
            Triangulator tr = new Triangulator(points2d);
            int[] indices = tr.Triangulate();

            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[selectionPoints.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(selectionPoints[i].x, 0, selectionPoints[i].z);
            }

            var uv = UvCalculator.CalculateUVs(vertices, 1f);

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.uv = uv;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            
            shapeSelection = msh;
            selectionPoints.Add(selectionPoints[0]);
            EditorWindow.GetWindow<Prefabshop>().maskShape = msh;
            EditorWindow.GetWindow<Prefabshop>().maskOutline = selectionPoints.ToArray();
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