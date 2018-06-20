using UnityEngine;

public static class VectorUtils
{
    public static float ToAngle(this Vector2 vector)
    {
        return Mathf.Atan2(vector.normalized.y, vector.normalized.x) * Mathf.Rad2Deg;
    }

    public static float ToAngle(this Vector3 vector)
    {
        return Mathf.Atan2(vector.normalized.y, vector.normalized.x) * Mathf.Rad2Deg;
    }
}