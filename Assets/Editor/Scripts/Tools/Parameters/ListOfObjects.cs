using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class ListOfObjects : Parameter
    {
        public List<GameObject> savedList;

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
        }
    }
}
