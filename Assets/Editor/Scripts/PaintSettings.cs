using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [System.Serializable]
    public class PaintSettings
    {
        public Color placeBrush = new Color(0, 1, 0, 0.1f);
        public float size = 10f;
        public int count = 1;
        public float gap = 0f;
        public string targetTag = "Untagged";
        public float randomScaleMin = 1;
        public float randomScaleMax = 1f;
        public int toolBar;
        public string[] menuOptions = new string[3] { "World", "Camera", "Normal" };

        public Transform targetParent = null;
        public int targetLayer;
        public LayerMask ignoringLayer;
        public bool checkObjeck;
    }
}
