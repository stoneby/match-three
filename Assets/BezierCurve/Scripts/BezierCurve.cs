using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
    private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    public static List<Vector3> GetBezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, int segment)
    {
        var result = new List<Vector3>();
        for (var i = 0; i <= segment; i++)
        {
            var t = i / (float)segment;
            var pixel = CalculateCubicBezierPoint(t, p1, p2, p2, p3);
            result.Add(pixel);
        }
        return result;
    }
}
