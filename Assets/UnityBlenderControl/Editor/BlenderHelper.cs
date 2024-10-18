
// using System;
// using System.Reflection;
using UnityEngine;

public static class BlenderHelper
{
    // not needed
    // private static Assembly EditorSnapSettingsAssembly;
    // private static Type EditorSnapSettings;
    // private static PropertyInfo scale, move, rotate;

    // public static void InitEditorSnapSettings()
    // {
    //     EditorSnapSettingsAssembly = Assembly.Load("UnityEditor.dll");
    //     EditorSnapSettings = EditorSnapSettingsAssembly.GetType("UnityEditor.EditorSnapSettings");
    //     scale = EditorSnapSettings.GetProperty("scale");
    //     move = EditorSnapSettings.GetProperty("move");
    //     rotate = EditorSnapSettings.GetProperty("rotate");
    // }

    public static float GetSnapScale()
    {
        return UnityEditor.EditorSnapSettings.scale;

    }
    public static Vector3 GetSnapMove()
    {
        return UnityEditor.EditorSnapSettings.move;
    }
    public static float GetSnapRotate()
    {
        return UnityEditor.EditorSnapSettings.rotate;
    }
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
    public static bool RightMouseHeld = false;
    public static void RightMouseHeldCheck()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 1) // Right mouse button
        {
            RightMouseHeld = true;
        }
        else if (e.type == EventType.MouseUp && e.button == 1) // Right mouse button released
        {
            RightMouseHeld = false;
        }
    }
    public static void CheckSnap()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown
        && (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl))
        {
            TransformModeManager.isSnappingEnabled = true;
        }
        else if (e.type == EventType.KeyUp
        && e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
        {
            TransformModeManager.isSnappingEnabled = false;
        }
    }
    public static bool CancelKeyPressed(Event e)
    {
        bool cancel = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            || (e.type == EventType.MouseDown && e.button == 0);
        if (cancel) 
        {
            e.Use();
        }
        return cancel;
    }
    public static bool RevertKeyPressed(Event e)
    {
        bool revert = (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            || (e.type == EventType.MouseDown && e.button == 1);
        if (revert) 
        {
            e.Use();
        }
        return revert;
    }

    public static KeyCode AxisKeycode(Event e)
    {
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.X || e.keyCode == KeyCode.Y || e.keyCode == KeyCode.Z)) {
            e.Use();
            return e.keyCode;
        }
        return KeyCode.None;
    }
    public static void AppendUnitNumber(Event e, ref string UnitNumber)
    {
        char input = e.character;
        if (e.isKey && char.IsDigit(input) || input == '-')
        {
            if (UnitNumber == "")
            {
                // If the input string is empty, prepend the character with a sign
                UnitNumber = '+' + input.ToString();
            }
            else if (input == '+' || input == '-')
            {
                // If a new sign is entered, get the previous sign and multiply it with the new sign
                int previousSign = UnitNumber[0] == '-' ? -1 : 1;
                int newSign = input == '-' ? -1 : 1;
                int resultSign = previousSign * newSign;
                // Replace the existing sign with the result
                UnitNumber = (resultSign == 1 ? "+" : "-") + UnitNumber.Substring(1);
            }
            else
            {
                // Otherwise, append the character
                UnitNumber += input;
            }
        }
    }
}
