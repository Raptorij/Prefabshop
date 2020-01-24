using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Rotation : Parameter
    {
        public bool usePrefabRotation;
        public bool randomRotation;

        Vector3 forceRotation;
        Vector3 plusRotation;
        Vector3 minRotation;
        Vector3 maxRotation;

        public Rotation(System.Type toolType) : base(toolType)
        {
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            GUI.enabled = !usePrefabRotation && !randomRotation;
            forceRotation = EditorGUILayout.Vector3Field("Force Rotation", forceRotation);
            GUI.enabled = true;
            usePrefabRotation = EditorGUILayout.Toggle("Use Prefab Rotation:", usePrefabRotation);
            GUI.enabled = !usePrefabRotation;
            GUI.enabled = randomRotation = EditorGUILayout.Toggle("Random Rotation:", randomRotation);
            minRotation = EditorGUILayout.Vector3Field("Min Val:", minRotation);
            maxRotation = EditorGUILayout.Vector3Field("Max Val:", maxRotation);
            GUI.enabled = true;
            plusRotation = EditorGUILayout.Vector3Field("Plus Rotation", plusRotation);
        }

        public Vector3 GetRotation(GameObject prefabRef)
        {
            var finalRotation = forceRotation;
            if (usePrefabRotation && prefabRef != null)
            {
                finalRotation = prefabRef.transform.eulerAngles;
            }
            if (!usePrefabRotation && randomRotation)
            {
                float x = Random.Range(minRotation.x, maxRotation.x);
                float y = Random.Range(minRotation.y, maxRotation.y);
                float z = Random.Range(minRotation.z, maxRotation.z);

                Vector3 randomed = new Vector3(x, y, z);
                finalRotation = randomed;
            }
            finalRotation += plusRotation;
            return finalRotation;
        }
    }
}
