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

        private int id = 0;

        public virtual int Identifier
        {
            get => id;
            set => id = value;
        }

        protected System.Type toolType;

        public System.Action valueChanged;

        protected Parameter(System.Type toolType)
        {
            this.toolType = toolType;
        }

        protected Parameter(System.Type toolType, int id)
        {
            this.toolType = toolType;
            this.id = id;
        }

        public virtual void DrawParameterGUI()
        {

        }

        public virtual void DrawTool(RaycastHit raycastHit)
        {

        }
    }
}