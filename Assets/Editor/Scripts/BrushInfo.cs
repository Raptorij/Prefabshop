using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Brush", menuName = "Prefabshop/Brush Info", order = 1)]
public class BrushInfo : ScriptableObject
{    
    public List<GameObject> brushObjects = new List<GameObject>();
    public PaintSettings settings;
}
