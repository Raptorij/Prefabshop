using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class ListOfObjects : Parameter
    {
        public override bool Hidden
        {
            get => true;
        }

        public List<GameObject> savedList;

        public ListOfObjects(Type toolType) : base(toolType)
        {
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
        }
    }
}
