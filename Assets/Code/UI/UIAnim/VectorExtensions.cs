
using UnityEngine;
using System.Collections;

public static class VectorExtensions
{
    public static float GetMaxX(Vector3[] vectors)
    {
        float val = -float.MaxValue;
        foreach (Vector3 v in vectors)
        {
            val = Mathf.Max(val, v.x);
        }

        return val;
    }

    public static float GetMinX(Vector3[] vectors)
    {
        float val = float.MaxValue;
        foreach (Vector3 v in vectors)
        {
            val = Mathf.Min(val, v.x);
        }

        return val;
    }

    public static float GetMaxY(Vector3[] vectors)
    {
        float val = -float.MaxValue;
        foreach (Vector3 v in vectors)
        {
            val = Mathf.Max(val, v.y);
        }

        return val;
    }

    public static float GetMinY(Vector3[] vectors)
    {
        float val = float.MaxValue;
        foreach (Vector3 v in vectors)
        {
            val = Mathf.Min(val, v.y);
        }

        return val;
    }

    public static float GetLargestChange(this Vector3 from, Vector3 to)
    {
        return Mathf.Abs(Mathf.Max(Mathf.Max(to.x / from.x, to.y / from.y), to.z / from.z));
    }

    public static bool Approximately(this Vector3 a, Vector3 b)
    {
        return a.x.Approximately(b.x) && a.y.Approximately(b.y) && a.z.Approximately(b.z);
    }

    public static bool Approximately(this Vector2 a, Vector2 b)
    {
        return a.x.Approximately(b.x) && a.y.Approximately(b.y);
    }

    public static bool Approximately(this float a, float b)
    {
        // Inlined version of Mathf.Approximately
        // 			return Mathf.Abs(b - a) < Mathf.Max(1E-06f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), Mathf.Epsilon * 8f);
        float c = b - a;
        float absc = c >= 0 ? c : -c;
        float absa = a >= 0 ? a : -a;
        float absb = b >= 0 ? b : -b;
        float maxab = absa <= absb ? absb : absa;
        float x = 1E-06f * maxab;
        float y = Mathf.Epsilon * 8f;
        float z = x <= y ? y : x;
        return absc < z;
    }

    public static Vector2 yx(this Vector2 v)
    {
        return new Vector2(v.y, v.x);
    }

    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 yz(this Vector3 v)
    {
        return new Vector2(v.y, v.z);
    }

    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 x0y(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static Vector2 xy(this Vector4 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 yz(this Vector4 v)
    {
        return new Vector2(v.y, v.z);
    }

    public static Vector2 xz(this Vector4 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector2 xw(this Vector4 v)
    {
        return new Vector2(v.x, v.w);
    }

    public static Vector2 yw(this Vector4 v)
    {
        return new Vector2(v.y, v.z);
    }

    public static Vector2 zw(this Vector4 v)
    {
        return new Vector2(v.x, v.w);
    }

    public static bool InScreenSpace(this Vector2 pos)
    {
        return pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.height;
    }
    
    public static Vector3 Mult(this Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
    }

    public static Vector2 Mult(this Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.x * rhs.x, lhs.y * rhs.y);
    }

    public static Vector2 Inverted(this Vector2 v)
    {
        return new Vector2(1 / v.x, 1 / v.y);
    }

    public static Vector3 Inverted(this Vector3 v)
    {
        return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
    }
}

public static class RectExtensions
{
    public static Vector3 NormalizedToPointUnclamped(this Rect rect, Vector2 pivot)
    {
        return rect.position + (rect.size.Mult(pivot));
    }

    public static Vector2 PointToNormalizedUnclamped(this Rect rect, Vector2 point)
    {
        return (point - rect.position).Mult(rect.size.Inverted());
    }
}
