using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.S)]
    public class SmudgeTool : Tool
    {
        public RaycastHit raycastHit;

        public SmudgeTool() : base()
        {
            var type = this.GetType();

            AddParameter(new Radius(type));
            AddParameter(new IgnoringLayer(type));
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
            Handles.color = new Color(0, 1, 0, 0.1f);
            Handles.DrawSolidDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
            Handles.color = Color.white;
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);
            var transformArray = GameObject.FindObjectsOfType<GameObject>()
                                .Where(t => Vector3.Distance(t.transform.position, drawPointHit.point) < GetParameter<Radius>().value)
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
                //GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
                //if (brushInfo.brushObjects.Contains(prefabAsset))
                //{
                //    go.transform.position += new Vector3(Event.current.delta.x,0, Event.current.delta.y);
                //}
            }
            onlyPrefabs.Clear();
        }
    }
}