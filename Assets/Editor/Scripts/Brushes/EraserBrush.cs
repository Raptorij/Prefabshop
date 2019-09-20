using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[BrushKeyCode(KeyCode.E)]
public class EraserBrush : Brush
{
    public EraserBrush(BrushInfo into, PaintSettings settings) : base(into, settings) { }

    public override void Paint(RaycastHit drawPointHit)
    {
        base.Paint(drawPointHit);
        var transformArray = GameObject.FindObjectsOfType<GameObject>()
                            .Where(t => Vector3.Distance(t.transform.position, drawPointHit.point) < paintSettings.size / 2f)
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
            var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(go);
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
            if (brushInfo.brushObjects.Contains(prefabAsset))
            {
                Undo.DestroyObjectImmediate(prefabInstance);
                Undo.DestroyObjectImmediate(go);
            }
        }
        onlyPrefabs.Clear();
    }
}
