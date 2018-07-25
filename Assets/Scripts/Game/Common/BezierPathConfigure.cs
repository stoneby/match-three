using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BezierPathConfigure : MonoBehaviour
{
    public PathType PlanePathType = PathType.CatmullRom;
    public Ease EaseType = Ease.InOutQuad;
    public int Segment = 20;
    public float Duration = 0.5f;

    public int MinAngle = 30;
    public int MaxAngle = 45;
    public int MinLen = 5;
    public int MaxLen = 8;
    public float BaseLen = 1.9f;

    public List<Vector3> GetPath(Vector3 p1, Vector3 p2)
    {
        var ran = PathHelper.GetRandomPoint(p1, p2, MinAngle, MaxAngle, MinLen, MaxLen, BaseLen);
        return BezierCurve.GetBezierCurve(p1, ran, p2, Segment);
    }
}
