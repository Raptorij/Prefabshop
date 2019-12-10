using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.M)]
    public class MagicWandTool : Tool
    {
        public RaycastHit raycastHit;

        public MagicWandTool(BrushInfo into) : base(into)
        {
            AddParameter(new Shape());
            AddParameter(new Radius());
            AddParameter(new Count());
            AddParameter(new Gap());
            AddParameter(new Tag());
            AddParameter(new Layer());
            AddParameter(new Parent());
            AddParameter(new IgnoringLayer());
            AddParameter(new ListOfObjects());
            AddParameter(new PrefabSelector());
            GetParameter<PrefabSelector>().buttonName = "Replace";
            GetParameter<PrefabSelector>().onButtonClick += ReplacePrefabs;
        }

        public override void SelectTool()
        {
            base.SelectTool();
        }

        public override void DeselectTool()
        {
            GetParameter<PrefabSelector>().onButtonClick = null;
            base.DeselectTool();
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit raycast;
            Physics.Raycast(castRay, out raycast,Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            SelectPrefabs(raycast);
        }

        private void SelectPrefabs(RaycastHit raycastHit)
        {
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(raycastHit.collider.gameObject) as GameObject;
            var listOfPrefabs = FindAllPrefabInstances(prefabAsset);
            GetParameter<ListOfObjects>().savedList = listOfPrefabs;
            Selection.objects = listOfPrefabs.ToArray();
        }

        void ReplacePrefabs()
        {
            var gameObjects = Selection.objects;
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject obj = gameObjects[i] as GameObject;
                var position = obj.transform.position;
                var rotation = obj.transform.rotation;
                CreateObject(position, rotation);
            }
            for (int i = 0; i < gameObjects.Length; i++)
            {
                Undo.DestroyObjectImmediate(gameObjects[i]);
            }
        }

        void CreateObject(Vector3 position, Quaternion rotation)
        {
            GameObject osd = PrefabUtility.InstantiatePrefab(GetParameter<PrefabSelector>().selectedPrefab) as GameObject;
            osd.transform.position = position;
            osd.transform.rotation = rotation;
            Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
        }

        List<GameObject> FindAllPrefabInstances(UnityEngine.Object myPrefab)
        {
            List<GameObject> result = new List<GameObject>();
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject GO in allObjects)
            {
                if (EditorUtility.GetPrefabType(GO) == PrefabType.PrefabInstance)
                {
                    UnityEngine.Object GO_prefab = EditorUtility.GetPrefabParent(GO);
                    if (myPrefab == GO_prefab)
                    {
                        result.Add(GO);
                    }
                }
            }
            return result;
        }
    }
}