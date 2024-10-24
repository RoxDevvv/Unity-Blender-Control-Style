
// using System;
// using System.Reflection;
using System.Globalization;
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
 
    public static Vector3 GetAxisVector(KeyCode keyCode) {
        if (TransformModeManager.swapYAndZ) {
            return keyCode switch {
                KeyCode.X => Vector3.right,
                KeyCode.Y => Vector3.forward,
                KeyCode.Z => Vector3.up,
                _ => Vector3.one
            }; 
        } else {
            return keyCode switch {
                KeyCode.X => Vector3.right,
                KeyCode.Y => Vector3.up,
                KeyCode.Z => Vector3.forward,
                _ => Vector3.one
            };
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
    public static void AppendUnitNumber(Event e, ref string unitNumber, ref bool isPositive)
    {
        if (!(e.type == EventType.KeyDown && e.isKey))
        {
            return;
        }

        char input = e.character;
        if (input == '-')
        {
            isPositive = !isPositive;
        }
        else if (char.IsDigit(input))
        {
            unitNumber += input;
        }
        else if (input == '.')
        {
            if (!unitNumber.Contains('.'))
            {
                unitNumber += input;
            }
        }
        else if (e.keyCode == KeyCode.Backspace)
        {
            if (unitNumber.Length != 0)
            {
                unitNumber = unitNumber.Substring(0, unitNumber.Length-1);
            }
        }
    }

    public static bool TryParseUnitNumber(string unitNumber, bool isPositive, out float parsedNumber)
    {
        // The CultureInfo must be specified to ensure that '.' is being used as the decimal point.
        if (float.TryParse(unitNumber, NumberStyles.Float, new CultureInfo("en-US"), out parsedNumber))
        {
            if (!isPositive)
            {
                parsedNumber = - parsedNumber;
            }
            return true;
        }
        return false;
    }
    
    public static bool IsModifierPressed(Event e) {
        return e.control || e.alt || e.shift;
    }
}
