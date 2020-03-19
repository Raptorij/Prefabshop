using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolColor(ToolColorAttribute.ToolUseType.Other)]
    [ToolKeyCodeAttribute(KeyCode.U)]
    public class MagnetTool : Tool
    {
        public RaycastHit raycastHit;
        private float lastInterval;
        private double editorDeltaTime;
        private double lastTimeSinceStartup;
        Material drawMat;
        private Mesh shape;

        public MagnetTool() : base()
        {
            var type = this.GetType();
            
            AddParameter(new PrefabsSet(type));
            AddParameter(new FloatParameter(type, "Strenght", 0));
            AddParameter(new Outer(type));
            AddParameter(new Radius(type));
            AddParameter(new Scale(type));
            AddParameter(new Ignore(type));
            AddParameter(new CachedGameObjects(type));
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

        protected override void DrawTool(Ray ray)
        {
            base.DrawTool(ray);
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<Ignore>().layer));
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

            if(shape == null)
            {
                var primitiveGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shape = primitiveGo.GetComponent<MeshFilter>().sharedMesh;
                primitiveGo.hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy;
                GameObject.DestroyImmediate(primitiveGo);
            }

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(raycastHit.point, Quaternion.identity, Vector3.one * GetParameter<Radius>().value * 2);
            if (drawMat == null)
            {
                drawMat = new Material(Shader.Find("Raptorij/BrushShapeZ"));
            }
            drawMat.SetColor("_Color", toolColor);
            drawMat.SetPass(0);
            Graphics.DrawMeshNow(shape, matrix, 0);

            Handles.color = Color.white;
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
        }

        protected override void OnStartPaint(RaycastHit startPointHit)
        {
            base.OnStartPaint(startPointHit);
            GetParameter<CachedGameObjects>().gameObjects.Clear();
            lastInterval = Time.realtimeSinceStartup;
            lastTimeSinceStartup = 0;
            EditorApplication.update += UpdateEditor;
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);
            var transformArray = GameObject.FindObjectsOfType<GameObject>()
                                .Where(t => Vector3.Distance(t.transform.position, drawPointHit.point) <= GetParameter<Radius>().value)
                                .ToArray();
            List<GameObject> onlyPrefabs = new List<GameObject>();
            foreach (var coll in transformArray)
            {
                if (PrefabUtility.GetPrefabInstanceHandle(coll.gameObject) != null)
                {
                    var prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(coll.gameObject);
                    if (!onlyPrefabs.Contains(prefab))
                    {
                        onlyPrefabs.Add(prefab);
                    }
                }
            }
            foreach (var go in onlyPrefabs)
            {
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
                if (GetParameter<PrefabsSet>().selectedPrefabs.Contains(prefabAsset))
                {
                    GetParameter<CachedGameObjects>().AddToСache(go);
                }
            }
            onlyPrefabs.Clear();
        }

        protected override void OnEndPaint(RaycastHit endPointHit)
        {
            base.OnEndPaint(endPointHit);
            GetParameter<CachedGameObjects>().gameObjects.Clear();
            lastTimeSinceStartup = 0;
            EditorApplication.update -= UpdateEditor;
        }

        private void UpdateEditor()
        {
            if (lastTimeSinceStartup == 0f)
            {
                lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            }
            editorDeltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
            lastTimeSinceStartup = EditorApplication.timeSinceStartup;

            var cached = GetParameter<CachedGameObjects>().gameObjects;
            if (cached.Count > 0)
            {
                Undo.RegisterCompleteObjectUndo(cached.ToArray(), "Magnet Objs");
                Undo.FlushUndoRecordObjects();
                for (int i = 0; i < cached.Count; i++)
                {
                    var heading = raycastHit.point - cached[i].transform.position;
                    var distance = heading.magnitude;
                    if (distance <= GetParameter<Radius>().value)
                    {
                        var direction = heading / distance;

                        int sign = GetParameter<Outer>().value ? -1 : 1;

                        cached[i].transform.position += direction * sign * (float)editorDeltaTime * GetParameter<FloatParameter>().value;                            
                    }
                    else
                    {
                        GetParameter<CachedGameObjects>().gameObjects.Remove(cached[i]);
                        return;
                    }
                }                    
            }
        }
    }
}
