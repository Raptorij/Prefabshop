using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [CreateAssetMenu(fileName = "PrefabSet", menuName = "Prefabshop/Prefab Set Info", order = 1)]
    public class PrefabSetInfo : ScriptableObject
    {
        public List<GameObject> brushObjects = new List<GameObject>();
    }
}