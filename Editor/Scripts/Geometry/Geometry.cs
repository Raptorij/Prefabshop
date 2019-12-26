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
}