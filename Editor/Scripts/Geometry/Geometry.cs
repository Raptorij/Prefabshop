using System;
using UnityEngine;

public static class Geometry
{
    public static Vector3[] GetSquareOverPoints(Vector3[] points)
    {
        float minX = Mathf.Infinity;
        float minZ = Mathf.Infinity;

        float maxX = -Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        for (int i = 0; i < points.Length; i++)
        {
            minX = points[i].x < minX ? points[i].x : minX;
            minZ = points[i].z < minZ ? points[i].z : minZ;

            maxX = points[i].x > maxX ? points[i].x : maxX;
            maxZ = points[i].z > maxZ ? points[i].z : maxZ;
        }

        Vector3 minXminZ = new Vector3(minX, 0, minZ);
        Vector3 maxXminZ = new Vector3(maxX, 0, minZ);
        Vector3 minXmaxZ = new Vector3(minX, 0, maxZ);
        Vector3 maxXmaxZ = new Vector3(maxX, 0, maxZ);

        return new Vector3[] { minXminZ, maxXminZ, minXmaxZ, maxXmaxZ };
    }

    public static bool PointInPolygon(float X, float Z, Vector3[] points)
    {
        int max_point = points.Length - 1;
        float total_angle = GetAngle(points[max_point].x, points[max_point].z,
                                    X, Z,
                                    points[0].x, points[0].z);
        
        for (int i = 0; i < max_point; i++)
        {
            total_angle += GetAngle(points[i].x, points[i].z,
                                    X, Z,
                                    points[i + 1].x, points[i + 1].z);
        }
        //return (Math.Abs(total_angle) > 0.000001);
        return (Math.Abs(total_angle) > 1);
    }
    
    public static float GetAngle(float Ax, float Ay,
                                float Bx, float By, 
                                float Cx, float Cy)
    {
        
        float dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);
        
        float cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

        return (float)Math.Atan2(cross_product, dot_product);
    }
    
    private static float DotProduct(float Ax, float Ay,
                                    float Bx, float By,
                                    float Cx, float Cy)
    {        
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;
        
        return (BAx * BCx + BAy * BCy);
    }

    public static float CrossProductLength(float Ax, float Ay,
                                            float Bx, float By, 
                                            float Cx, float Cy)
    {        
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;
    
        return (BAx * BCy - BAy * BCx);
    }

    public static Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        //if you're repeatedly doing this on a single mesh, you'll likely want to cache cumulativeSizes and total
        float[] sizes = GetTriSizes(mesh.triangles, mesh.vertices);
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        //so everything above this point wants to be factored out

        float randomsample = UnityEngine.Random.value * total;

        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1)
            Debug.LogError("triIndex should never be -1");

        Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
        Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

        //generate random barycentric coordinates

        float r = UnityEngine.Random.value;
        float s = UnityEngine.Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }
        //and then turn them back to a Vector3
        Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
        return pointOnMesh;

    }

    static float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
        }
        return sizes;
    }
}