using DG.Tweening;
using UnityEngine;

public class PlanePathTest : MonoBehaviour
{
    public Transform P1;
    public Transform P2;
    public Transform P3;

    public int Segment = 20;
    public PathType PlanePathType;
    public float Duration;
    public Vector3 PlaneForward;
    public Vector3 PlaneUp;

    public int MinAngle;
    public int MaxAngle;
    public int MinLen;
    public int MaxLen;
    public int BaseLen;

    public void OnGUI()
    {
        if (GUILayout.Button("Play"))
        {
            var path = BezierCurve.GetBezierCurve(P1.position, P2.position, P3.position, Segment);
            transform.DOPath(path.ToArray(), Duration, PlanePathType);
        }

        if (GUILayout.Button("GenerateMiddlePoint"))
        {
            var middle = PathHelper.GetMiddlePoint(P1.position, P3.position);
            var path = BezierCurve.GetBezierCurve(P1.position, middle, P3.position, Segment);
            transform.DOPath(path.ToArray(), Duration, PlanePathType);
        }

        if (GUILayout.Button("RandomPoint"))
        {
            var ran = PathHelper.GetRandomPoint(P1.position, P3.position, MinAngle, MaxAngle, MinLen, MaxLen, BaseLen);
            var path = BezierCurve.GetBezierCurve(P1.position, ran, P3.position, Segment);
            transform.DOPath(path.ToArray(), Duration, PlanePathType);
        }
    }
}
