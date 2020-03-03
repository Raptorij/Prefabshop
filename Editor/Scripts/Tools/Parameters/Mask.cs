using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Mask : Parameter
    {
        private Prefabshop prefabshop;

        bool haveMask;
        public bool HaveMask
        {
            get
            {
                haveMask = prefabshop.maskShape != null;
                return haveMask;
            }
        }

        private Mesh maskShape;
        public Mesh MaskShape
        {
            get
            {
                maskShape = prefabshop.maskShape;
                return maskShape;
            }
        }

        public Mask(Type toolType) : base(toolType)
        {
            Hidden = true;
            var previousFocus = EditorWindow.focusedWindow;
            prefabshop = EditorWindow.GetWindow<Prefabshop>();
            haveMask = prefabshop.maskShape != null;
            maskShape = prefabshop.maskShape;            
            previousFocus.Focus();
        }        

        public bool CheckPoint(Vector3 point)
        {
            if (prefabshop.maskShape != null)
            {
                var maskOutline = prefabshop.maskOutline;
                return Geometry.PointInPolygon(point.x, point.z, maskOutline);
            }
            else
            {
                return true;
            }
        }
    }
}