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

        private bool enable = true;

        public virtual bool Enable
        {
            get => enable;
            set => enable = value;
        }

        public System.Action OnValueChange;

        public virtual void DrawParameterGUI()
        {

        }

        public virtual void DrawTool(RaycastHit raycastHit)
        {

        }
    }
}