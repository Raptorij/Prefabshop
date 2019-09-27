using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideInInspector]
public class GizmosDrawer : MonoBehaviour
{
    public System.Action onDrawGizmos;

    private void OnDrawGizmos()
    {
        onDrawGizmos?.Invoke();
    }
}
