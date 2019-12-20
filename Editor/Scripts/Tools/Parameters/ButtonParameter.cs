using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class ButtonParameter : Parameter
    {
        public string buttonName;
        public Action<string> onButtonClick;

        public ButtonParameter(Type toolType) : base(toolType)
        {
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            if (GUILayout.Button(buttonName))
            {
                onButtonClick?.Invoke(buttonName);
            }
        }
    }
}