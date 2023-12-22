
using UnityEngine;

public static class BlenderHelper
{
    public static Vector3 GetObjectAxis(Transform target, Vector3 VectorAxis)
    {
        if (VectorAxis == Vector3.right)
        {
            return target.right;
        }
        else if (VectorAxis == Vector3.up)
        {
            return target.up;
        }
        else if (VectorAxis == Vector3.forward)
        {
            return target.forward;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public static Vector3 GetAxisVector(KeyCode keyCode)
    {
        if (keyCode == KeyCode.X)
        {
            return Vector3.right;
        }
        else if (keyCode == KeyCode.Y)
        {
            return Vector3.up;
        }
        else if (keyCode == KeyCode.Z)
        {
            return Vector3.forward;
        }
        else
        {
            return Vector3.one;
        }
    }
    public static bool CancelKeyPressed(Event e)
    {
        return e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
       || (e.type == EventType.MouseDown && e.button == 0);
    }
    public static bool RevertKeyPressed(Event e)
    {
        return e.type == EventType.MouseDown && e.button == 1;
    }

    public static KeyCode AxisKeycode(Event e)
    {
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.X || e.keyCode == KeyCode.Y || e.keyCode == KeyCode.Z))
            return e.keyCode;
        return KeyCode.None;
    }
}
