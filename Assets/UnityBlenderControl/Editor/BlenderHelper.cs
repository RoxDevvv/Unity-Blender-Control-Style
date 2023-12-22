
using UnityEngine;

public static class BlenderHelper
{
    public static Vector3 GetObjectAxis(Transform target, Vector3 VectorAxis)
    {
        return VectorAxis switch
        {
            Vector3 right when right == Vector3.right => target.right,
            Vector3 up when up == Vector3.up => target.up,
            Vector3 forward when forward == Vector3.forward => target.forward,
            _ => Vector3.zero,
        };
    }
    public static Vector3 GetAxisVector(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.X => Vector3.right,
            KeyCode.Y => Vector3.up,
            KeyCode.Z => Vector3.forward,
            _ => Vector3.one,
        };
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
