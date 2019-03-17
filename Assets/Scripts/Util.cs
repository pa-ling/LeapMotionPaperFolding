using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util {

    public static void DebugRay(Vector3 origin, Vector3 destination, Color color)
    {
        Debug.DrawRay(origin, Vector3.Normalize(destination - origin) * Vector3.Distance(origin, destination), color, 0, true);
    }

    public static void DebugPoint(Vector3 point, Color color)
    {
        Debug.DrawLine(point - 0.01f * Vector3.forward, point + 0.01f * Vector3.forward, color);
        Debug.DrawLine(point - 0.01f * Vector3.right, point + 0.01f * Vector3.right, color);
    }

    public static Vector3 RotateAroundAxis(Vector3 v, float a, Vector3 axis, bool bUseRadians = false)
    {
        if (bUseRadians) a *= Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(a, axis);
        return q * v;
    }

    public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize(); //this needs to be a unit vector
        Vector3 v = pnt - linePnt;
        float d = Vector3.Dot(v, lineDir);

        return linePnt + lineDir * d;
    }

    public static int BoolToInt(bool value)
    {
        if (value)
            return 1;
        return 0;
    }

}
