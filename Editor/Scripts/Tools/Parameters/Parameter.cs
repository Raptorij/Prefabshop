using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public abstract class Parameter : IParameter
    {
        private bool hidden = false;

        public virtual bool Hidden
        {
            get => hidden;
            set => hidden = value;
        }

        private bool enable = true;

        public virtual bool Enable
        {
            get => enable;
            set => enable = value;
        }


        protected System.Type toolType;

        public System.Action OnValueChange;

        protected Parameter(System.Type toolType)
        {
            this.toolType = toolType;
        }

        public virtual void DrawParameterGUI()
        {

        }

        public virtual void DrawTool(RaycastHit raycastHit)
        {

        }
    }
}