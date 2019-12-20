using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public interface IParameter
    {
        bool Hidden
        {
            get;
            set;
        }

        bool Enable
        {
            get;
            set;
        }
    }
}