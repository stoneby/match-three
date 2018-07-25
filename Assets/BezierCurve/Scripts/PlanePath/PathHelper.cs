using UnityEngine;

public class PathHelper
{
    private static float Sin60 = 1.732f / 2;
    private static float Sin30 = 0.5f;

    private static Matrix4x4 rotate60Matrix = new Matrix4x4(
        new Vector4(Sin30, Sin60),
        new Vector4(-Sin60, Sin30),
        new Vector4(0, 0, 1, 0),
        new Vector4(0, 0, 0, 1));

    /// <summary>
    /// Get middle point in the vertical segement of p1p2.
    /// </summary>
    /// <param name="p1">point 1</param>
    /// <param name="p2">point 2</param>
    /// <returns>random middle point</returns>
    public static Vector3 GetMiddlePoint(Vector3 p1, Vector3 p2)
    {
        var vec = p2 - p1;
        var result = rotate60Matrix * vec;
        var random = Random.Range(0, 2);
        return (random > 0) ? (p1 + (Vector3)result) : (p1 - (Vector3)result);
    }

    public static Vector3 GetRandomPoint(Vector3 p1, Vector3 p2, int minAngle, int maxAngle, int minLen, int maxLen, float baseLen)
    {
        var angle = Random.Range(minAngle, maxAngle);
        var q = Quaternion.Euler(0, 0, angle);
        var matrix = Matrix4x4.Rotate(q);
        var vec = p2 - p1;
        var result = matrix * vec;
        result.Normalize();
        var len = baseLen * Random.Range(minLen, maxLen);
        return ((Vector3) result) * len + p1;
    }
}
