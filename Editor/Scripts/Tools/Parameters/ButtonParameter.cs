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

        public ButtonParameter(Type toolType, string name, int id) : base(toolType)
        {
            this.buttonName = name;
            this.Identifier = id;

            string saveId = $"[Prefabshop] {toolType.Name}.{this.GetType().Name}.{buttonName}.{Identifier}";
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