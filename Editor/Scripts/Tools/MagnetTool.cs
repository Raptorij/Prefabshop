using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.K)]
    public class MagnetTool : Tool
    {
        public RaycastHit raycastHit;
        private float lastInterval;

        public MagnetTool() : base()
        {
            var type = this.GetType();
            
            AddParameter(new PrefabsSet(type));
            AddParameter(new Outer(type));
            AddParameter(new Radius(type));
            AddParameter(new Scale(type));
            AddParameter(new IgnoringLayer(type));
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

            Handles.color = new Color(0, 1, 0, 0.1f);
            Handles.SphereHandleCap(0, raycastHit.point, Quaternion.identity, GetParameter<Radius>().value * 2, EventType.Repaint);
            Handles.color = Color.white;
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
        }

        protected override void OnStartPaint(RaycastHit startPointHit)
        {
            base.OnStartPaint(startPointHit);
            GetParameter<CachedGameObjects>().gameObjects.Clear();
            lastInterval = Time.realtimeSinceStartup;
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
            EditorApplication.update -= UpdateEditor;
        }

        private void UpdateEditor()
        {
            float timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + 0.001f)
            {
                var cached = GetParameter<CachedGameObjects>().gameObjects;
                if (cached.Count > 0)
                {
                    for (int i = 0; i < cached.Count; i++)
                    {
                        var heading = raycastHit.point - cached[i].transform.position;
                        var distance = heading.magnitude;
                        if (distance <= GetParameter<Radius>().value)
                        {
                            var direction = heading / distance;

                            int sign = GetParameter<Outer>().value ? -1 : 1;

                            cached[i].transform.position += direction * sign * (timeNow - lastInterval);
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
}
