using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class Mask : Parameter
    {
        public bool isEnabled;

        public Mask(Type toolType) : base(toolType)
        {
            Hidden = true;
        }        

        public bool CheckPoint(Vector3 point)
        {
            var previousFocus = EditorWindow.focusedWindow;
            if (EditorWindow.GetWindow<Prefabshop>().maskShape != null)
            {
                var maskOutline = EditorWindow.GetWindow<Prefabshop>().maskOutline;
                previousFocus.Focus();
                return Geometry.PointInPolygon(point.x, point.z, maskOutline);
            }
            else
            {
                previousFocus.Focus();
                return true;
            }
        }
    }
}