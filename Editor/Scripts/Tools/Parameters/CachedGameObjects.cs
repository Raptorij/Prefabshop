using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class CachedGameObjects : Parameter
    {
        public override bool Hidden
        {
            get => true;
        }

        public List<GameObject> gameObjects = new List<GameObject>();

        public CachedGameObjects(System.Type type) : base(type)
        {

        }

        public void AddToСache(GameObject obj)
        {
            if (!gameObjects.Contains(obj))
            {
                gameObjects.Add(obj);
            }
        }
    }
}