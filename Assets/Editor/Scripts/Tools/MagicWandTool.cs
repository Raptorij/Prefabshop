using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.M)]
    public class MagicWandTool : Tool
    {
        System.Action onReplace;
        GameObject underMouse;

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
            AddParameter(new PrefabSelector(true, "Replace", onReplace));
            GetParameter<PrefabSelector>().buttonName = "Replace";
        }

        public override void SelectTool()
        {
            base.SelectTool();
            underMouse = null;
            onReplace += ReplacePrefabs;
        }

        public override void DeselectTool()
        {
            onReplace -= ReplacePrefabs;
            underMouse = null;
            base.DeselectTool();
        }

        public override void DrawHandle(Ray drawPointHit)
        {
            base.DrawHandle(drawPointHit);

            if (Event.current.type == EventType.MouseMove)
            {
                underMouse = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            }
            if (underMouse != null)
            {
                if (PrefabUtility.GetOutermostPrefabInstanceRoot(underMouse) != null)
                {
                    underMouse = PrefabUtility.GetOutermostPrefabInstanceRoot(underMouse);
                    var shape = underMouse.GetComponentsInChildren<MeshFilter>();
                    var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                    mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                    mat.SetPass(0);
                    for (int i = 0; i < shape.Length; i++)
                    {
                        var position = shape[i].transform.position;
                        var rotation = shape[i].transform.rotation;
                        var scale = shape[i].transform.lossyScale;


                        var matrix = new Matrix4x4();
                        matrix.SetTRS(position, rotation, scale);
                        Graphics.DrawMeshNow(shape[i].sharedMesh, matrix, 0);
                    }
                }
            }
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var go = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            SelectPrefabs(go);
        }

        private void SelectPrefabs(GameObject objectUnderMouse)
        {
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(objectUnderMouse) as GameObject;
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

        List<GameObject> FindAllPrefabInstances(Object myPrefab)
        {
            List<GameObject> result = new List<GameObject>();
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var GO in allObjects)
            {
                if (PrefabUtility.GetOutermostPrefabInstanceRoot(GO) != null)
                {
                    var GO_prefab = PrefabUtility.GetCorrespondingObjectFromSource(GO);
                    if (myPrefab == GO_prefab)
                    {
                        result.Add(PrefabUtility.GetOutermostPrefabInstanceRoot(GO));
                    }
                }
            }
            return result;
        }
    }
}