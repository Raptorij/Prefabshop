using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [CreateAssetMenu(fileName = "Brush", menuName = "Prefabshop/Brush Info", order = 1)]
    public class BrushInfo : ScriptableObject
    {
        public List<GameObject> brushObjects = new List<GameObject>();
        public PaintSettings settings;
    }
}