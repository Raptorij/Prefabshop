using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.M)]
    public class MagicWandTool : Tool
    {
        System.Action onReplace;
        GameObject underMouse;
        int toolBarId;

        public MagicWandTool() : base()
        {
            var type = this.GetType();
            AddParameter(new SelectToolBar(type));
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new Parent(type));
            AddParameter(new IgnoringLayer(type));
            AddParameter(new ListOfObjects(type));
            AddParameter(new PrefabsSet(type));
            AddParameter(new Rotation(type));
            AddParameter(new ToggleParameter(type, "Save Hierarchy Position", 0));
            AddParameter(new ButtonParameter(type, "Replace Selected", 0));
            AddParameter(new Mask(type));

            GetParameter<SelectToolBar>().toolBar = new string[] { "Prefabs", "Mesh" };
            GetParameter<SelectToolBar>().onChangeToolBar += OnChangeToolBar;
            OnChangeToolBar(GetParameter<SelectToolBar>().idSelect);
            
            GetParameter<PrefabsSet>().Activate();
        }

        public void OnChangeToolBar(int id)
        {
            toolBarId = id;
        }

        public override void SelectTool()
        {
            base.SelectTool();
            underMouse = null;
            GetParameter<ButtonParameter>().onButtonClick += ReplacePrefabs;
        }

        public override void DeselectTool()
        {
            GetParameter<ButtonParameter>().onButtonClick -= ReplacePrefabs;
            underMouse = null;
            base.DeselectTool();
        }

        public override void OnGUI()
        {
            base.OnGUI();
            bool canReplace = GetParameter<PrefabsSet>().setInfo != null && GetParameter<PrefabsSet>().GetSelectedPrefabs().Count > 0;
            GetParameter<Rotation>().Enable = canReplace;
            GetParameter<ToggleParameter>(0).Enable = canReplace;
            GetParameter<ButtonParameter>(0).Enable = canReplace;
        }

        protected override void DrawTool(Ray drawPointHit)
        {
            base.DrawTool(drawPointHit);

            if (Event.current.type == EventType.MouseMove)
            {
                underMouse = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            }
            if (underMouse != null)
            {
                switch (toolBarId)
                {
                    case 0:
                        HeightLightPrefabs();
                        break;
                    case 1:
                        HeightLightMeshes();
                        break;
                }
            }
            if ((Event.current.type == EventType.MouseDown) && Event.current.button == 0)
            {
                switch (toolBarId)
                {
                    case 0:
                        SelectPrefabs(underMouse);
                        break;
                    case 1:
                        SelectMeshes(underMouse);
                        break;
                }
            }
        }

        void HeightLightPrefabs()
        {
            if (PrefabUtility.GetOutermostPrefabInstanceRoot(underMouse) != null)
            {
                underMouse = PrefabUtility.GetOutermostPrefabInstanceRoot(underMouse);
                var shape = underMouse.GetComponentsInChildren<MeshFilter>();
                Material mat = new Material(Shader.Find("Raptorij/BrushShape"));
                mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                mat.SetPass(0);
                for (int i = 0; i < shape.Length; i++)
                {
                    var position = shape[i].transform.position;
                    var rotation = shape[i].transform.rotation;
                    var scale = shape[i].transform.lossyScale;

                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetTRS(position, rotation, scale);
                    Graphics.DrawMeshNow(shape[i].sharedMesh, matrix, 0);
                }
            }
        }

        void HeightLightMeshes()
        {
            var shape = underMouse.GetComponentsInChildren<MeshFilter>();
            Material mat = new Material(Shader.Find("Raptorij/BrushShape"));
            mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
            mat.SetPass(0);
            for (int i = 0; i < shape.Length; i++)
            {
                var position = shape[i].transform.position;
                var rotation = shape[i].transform.rotation;
                var scale = shape[i].transform.lossyScale;

                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(position, rotation, scale);
                Graphics.DrawMeshNow(shape[i].sharedMesh, matrix, 0);
            }
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);
        }

        private void SelectPrefabs(GameObject objectUnderMouse)
        {
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(objectUnderMouse) as GameObject;
            var listOfPrefabs = FindAllPrefabInstances(prefabAsset);
            if (Event.current.shift)
            {
                GetParameter<ListOfObjects>().savedList.AddRange(listOfPrefabs);
            }
            else
            {
                GetParameter<ListOfObjects>().savedList = listOfPrefabs;
            }
            Selection.objects = GetParameter<ListOfObjects>().savedList.ToArray();
        }

        private void SelectMeshes(GameObject objectUnderMouse)
        {
            if (objectUnderMouse != null)
            {
                var meshRef = objectUnderMouse.GetComponentInChildren<MeshFilter>().sharedMesh;
                var meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
                List<GameObject> listOfMeshes = new List<GameObject>();
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    if (meshFilters[i].sharedMesh == meshRef)
                    {
                        listOfMeshes.Add(meshFilters[i].gameObject);
                    }
                }
                if (Event.current.shift)
                {
                    GetParameter<ListOfObjects>().savedList.AddRange(listOfMeshes);
                }
                else
                {
                    GetParameter<ListOfObjects>().savedList = listOfMeshes;
                }
                Selection.objects = GetParameter<ListOfObjects>().savedList.ToArray();
            }
        }

        void ReplacePrefabs(string buttonName)
        {
            GetParameter<ListOfObjects>().savedList.Clear();
            var prefabs = GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                var gameObjects = Selection.objects;
                for (int i = 0; i < gameObjects.Length; i++)
                {
                    CreateObject(gameObjects[i] as GameObject);
                }
                for (int i = 0; i < gameObjects.Length; i++)
                {
                    Undo.DestroyObjectImmediate(gameObjects[i]);
                }
            }
            else
            {
                Debug.Log($"<color=magenta>[Prefabshop] </color> There is no selected any objects in Options");
            }
        }

        void CreateObject(GameObject refObject)
        {
            var prefabs = GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                GameObject osd = PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
                osd.transform.position = refObject.transform.position;
                var getRotation = GetParameter<Rotation>().GetRotation(refObject);
                var finalEulerAngles = getRotation != Vector3.zero ? getRotation : refObject.transform.eulerAngles;
                osd.transform.eulerAngles = finalEulerAngles;
                if (GetParameter<ToggleParameter>().value)
                {
                    osd.transform.SetParent(refObject.transform.parent, true);
                }
                else if (GetParameter<Parent>().value != null)
                {
                    osd.transform.SetParent(GetParameter<Parent>().value, true);
                }
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
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
                        var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(GO);
                        if (GetParameter<Mask>().HaveMask)
                        {
                            if (GetParameter<Mask>().CheckPoint(prefabRoot.transform.position))
                            {
                                result.Add(prefabRoot);
                            }
                        }
                        else
                        {
                            result.Add(prefabRoot);
                        }
                    }
                }
            }
            return result;
        }
    }
}