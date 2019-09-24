using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public abstract class Parameter
    {
        public virtual void DrawParameterGUI()
        {

        }

        public virtual void DrawTool(RaycastHit raycastHit)
        {

        }
    }
}