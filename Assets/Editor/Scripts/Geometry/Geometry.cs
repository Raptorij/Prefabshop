using System;
using UnityEngine;

public static class Geometry
{
    public static bool PointInTriangle(Point A, Point B, Point C, Point P)
    {
        double s1 = C.y - A.y;
        double s2 = C.x - A.x;
        double s3 = B.y - A.y;
        double s4 = P.y - A.y;

        double w1 = (A.x * s1 + s4 * s2 - P.x * s1) / (s3 * s2 - (B.x - A.x) * s1);
        double w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }

    public static bool PointInTriangle(Vector3[] TriangleVectors, Vector3 P)
    {
        Vector3 A = TriangleVectors[0], B = TriangleVectors[1], C = TriangleVectors[2];
        if (SameSide(P, A, B, C) && SameSide(P, B, A, C) && SameSide(P, C, A, B))
        {
            Vector3 vc1 = Vector3.Cross((A - B), (A - C));
            if (Math.Abs(Vector3.Dot((A - P), vc1)) <= .01f)
            {
                return true;
            }
        }

        return false;
    }

    private static bool SameSide(Vector3 p1, Vector3 p2, Vector3 A, Vector3 B)
    {
        Vector3 cp1 = Vector3.Cross((B - A), (p1 - A));
        Vector3 cp2 = Vector3.Cross((B - A), (p2 - A));
        if (Vector3.Dot(cp1, cp2) >= 0)
        {
            return true;
        }
        return false;

    }
}

public struct Point
{
    public readonly double x;
    public readonly double y;

    public Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
}