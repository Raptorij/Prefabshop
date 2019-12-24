using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.C)]
    public class EraserTool : Tool
    {
        public bool byPrefabSet;

        public EraserTool() : base()
        {
            var type = this.GetType();

            AddParameter(new SelectToolBar(type));
            AddParameter(new PrefabsSet(type));
            AddParameter(new Radius(type));
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new IgnoringLayer(type));
            AddParameter(new CachedGameObjects(type));
            OnEndPaint += EndPaint;

            GetParameter<SelectToolBar>().toolBar = new string[] { "By PrefabsSet", "All Prefabs" };
            GetParameter<SelectToolBar>().onChangeToolBar += OnChangeToolBar;
            OnChangeToolBar(GetParameter<SelectToolBar>().idSelect);
            GetParameter<PrefabsSet>().Activate();
        }

        public void OnChangeToolBar(int id)
        {
            switch (id)
            {
                case 0:
                    byPrefabSet = true;
                    GetParameter<PrefabsSet>().Enable = true;
                    break;
                case 1:
                    byPrefabSet = false;
                    GetParameter<PrefabsSet>().Enable = false;
                    break;                   
            }
        }

        public override void DrawHandle(Ray ray)
        {
            var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            var raycastHit = casts[casts.Length - 1];
            Handles.color = new Color(1, 0, 0, 0.25f);
            Handles.SphereHandleCap(0,raycastHit.point, Quaternion.identity, GetParameter<Radius>().value * 2, EventType.Repaint);
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
                //var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(go);
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
                if (byPrefabSet)
                {
                    if (GetParameter<PrefabsSet>().selectedPrefabs.Contains(prefabAsset))
                    {
                        go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                        go.SetActive(false);
                        GetParameter<CachedGameObjects>().AddToСache(go);
                    }
                }
                else
                {
                    go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                    go.SetActive(false);
                    GetParameter<CachedGameObjects>().AddToСache(go);
                }
            }
            onlyPrefabs.Clear();
        }

        private void EndPaint(RaycastHit raycastHit)
        {
            EditorApplication.update += RemoveCechedObjects;
        }

        void RemoveCechedObjects()
        {
            var cached = GetParameter<CachedGameObjects>().gameObjects;
            for (int i = 0; i < cached.Count; i++)
            {
                if (cached[i] == null)
                {
                    cached.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < cached.Count; i++)
            {
                var go = cached[0];
                if (go != null)
                {
                    go.hideFlags = HideFlags.None;
                    go.SetActive(true);
                    Undo.DestroyObjectImmediate(go);
                }
                cached.RemoveAt(0);
            }
            if (cached.Count <= 0)
            {
                EditorApplication.update -= RemoveCechedObjects;
            }
        }
    }    
}