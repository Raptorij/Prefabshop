using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    public Texture2D texture;
    public Mesh shape;
    public Material mat;
    public float size;

    // Start is called before the first frame update
    void Start()
    {
        var t = texture;
        var textureMap = new int[t.width, t.height];
        for (int i = 0; i < textureMap.GetLength(0); i++)
        {
            for (int j = 0; j < textureMap.GetLength(1); j++)
            {
                textureMap[i, j] = t.GetPixel(i, j).r >= .6f ? 1 : 0;
            }
        }
        MarchingSquares ms = new MarchingSquares();
        shape = ms.GenerateMesh(textureMap, size);
    }

    private void Update()
    {
        if (shape != null)
        {
            Graphics.DrawMesh(shape, Vector3.zero, Quaternion.identity, mat, 0);
        }
    }
}
