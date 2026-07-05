using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class KeyBindings
{
    public enum Action
    {
        Move,
        Rotate,
        Scale,
        AxisX,
        AxisY,
        AxisZ,
        DrawModePieMenu,
        PivotPointPieMenu,
    }

    private static Dictionary<Action, KeyCode> bindings = new Dictionary<Action, KeyCode>();

    private static readonly Dictionary<Action, KeyCode> defaults = new Dictionary<Action, KeyCode>
    {
        { Action.Move, KeyCode.G },
        { Action.Rotate, KeyCode.R },
        { Action.Scale, KeyCode.S },
        { Action.AxisX, KeyCode.X },
        { Action.AxisY, KeyCode.Y },
        { Action.AxisZ, KeyCode.Z },
        { Action.DrawModePieMenu, KeyCode.D },
        { Action.PivotPointPieMenu, KeyCode.Period },
    };

    public static KeyCode GetBinding(Action action)
    {
        if (bindings.TryGetValue(action, out KeyCode key))
            return key;
        return defaults[action];
    }

    public static void SetBinding(Action action, KeyCode key)
    {
        bindings[action] = key;
        Save();
    }

    public static void ResetToDefaults()
    {
        foreach (var kvp in defaults)
            bindings[kvp.Key] = kvp.Value;
        Save();
    }

    public static void Load()
    {
        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            string prefKey = $"BlenderControlKey_{action}";
            int intVal = EditorPrefs.GetInt(prefKey, (int)defaults[action]);
            bindings[action] = (KeyCode)intVal;
        }
    }

    public static void Save()
    {
        foreach (var kvp in bindings)
        {
            EditorPrefs.SetInt($"BlenderControlKey_{kvp.Key}", (int)kvp.Value);
        }
    }

    public static string ActionDisplayName(Action action)
    {
        return action switch
        {
            Action.Move => "Move (Grab)",
            Action.Rotate => "Rotate",
            Action.Scale => "Scale",
            Action.AxisX => "Constrain X Axis",
            Action.AxisY => "Constrain Y Axis",
            Action.AxisZ => "Constrain Z Axis",
            Action.DrawModePieMenu => "Draw Mode Pie Menu",
            Action.PivotPointPieMenu => "Pivot Point Pie Menu",
            _ => action.ToString(),
        };
    }
}
