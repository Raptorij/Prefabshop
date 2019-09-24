using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public abstract class Parameter : IParameter
    {
        public virtual bool Hidden
        {
            get => false;
        }

        public virtual void DrawParameterGUI()
        {

        }

        public virtual void DrawTool(RaycastHit raycastHit)
        {

        }
    }
}