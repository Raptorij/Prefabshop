using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Packages.PrefabshopEditor
{
    public class InstatiatePrefab : Parameter
    {
        public override bool Hidden
        {
            get => true;            
        }

        public InstatiatePrefab(Type toolType) : base(toolType)
        {

        }

        public void CreateObject(RaycastHit rayHit, Tool tool)
        {
            var newPos = rayHit.point + rayHit.normal.normalized * tool.GetParameter<Gap>().value;
            var prefabs = tool.GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                var selectedPrefab = prefabs[Random.Range(0, prefabs.Count)];
                GameObject osd = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
                osd.transform.position = newPos;
                if (tool.GetParameter<Scale>().randomScale)
                {
                    osd.transform.localScale *= Random.Range(tool.GetParameter<Scale>().minValue, tool.GetParameter<Scale>().maxValue);
                }
                osd.transform.SetParent(tool.GetParameter<Parent>().value);
                if (tool.GetParameter<Rotation>().useHitNormal)
                {
                    osd.transform.up = rayHit.normal;
                }
                osd.transform.eulerAngles = tool.GetParameter<Rotation>().GetRotation(selectedPrefab);
                osd.tag = tool.GetParameter<Tag>().value;
                osd.layer = tool.GetParameter<Layer>().value;
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
        }

        public void CreateObject(Vector3 pos, Tool tool)
        {
            var newPos = pos + Vector3.up * tool.GetParameter<Gap>().value;
            var prefabs = tool.GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                var selectedPrefab = prefabs[Random.Range(0, prefabs.Count)];
                GameObject osd = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
                osd.transform.position = newPos;
                if (tool.GetParameter<Scale>().randomScale)
                {
                    osd.transform.localScale *= Random.Range(tool.GetParameter<Scale>().minValue, tool.GetParameter<Scale>().maxValue);
                }
                osd.transform.up = selectedPrefab.transform.up;
                osd.transform.SetParent(tool.GetParameter<Parent>().value);
                osd.transform.eulerAngles = tool.GetParameter<Rotation>().GetRotation(selectedPrefab);
                osd.tag = tool.GetParameter<Tag>().value;
                osd.layer = tool.GetParameter<Layer>().value;
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
        }
    }
}