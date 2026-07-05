using System.Linq;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class PluginControlWindow : EditorWindow
{
    private static KeyBindings.Action? pendingRebind = null;
    private Vector2 scrollPos;

    private static readonly KeyBindings.Action[] bindableActions = new KeyBindings.Action[]
    {
        KeyBindings.Action.Move, KeyBindings.Action.Rotate, KeyBindings.Action.Scale,
        KeyBindings.Action.AxisX, KeyBindings.Action.AxisY, KeyBindings.Action.AxisZ,
        KeyBindings.Action.DrawModePieMenu, KeyBindings.Action.PivotPointPieMenu,
    };

    [MenuItem("Blender/Blender Control Window")]
    public static void ShowWindow()
    {
        GetWindow<PluginControlWindow>("Blender Plugin Control");
    }

    private void OnEnable()
    {
        LoadSettings();
        pendingRebind = null;
    }

    private void OnGUI()
    {
        GUILayout.Label("Blender Plugin Control", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        isBlenderPluginEnabled = EditorGUILayout.Toggle("Enable Plugin", isBlenderPluginEnabled);
        swapYAndZ = EditorGUILayout.Toggle("Swap Y and Z", swapYAndZ);

        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings();
        }

        GUILayout.Space(10);
        GUILayout.Label("Key Bindings", EditorStyles.boldLabel);
        GUILayout.Space(4);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var action in bindableActions)
        {
            DrawKeyBindingRow(action);
        }

        GUILayout.Space(6);

        if (GUILayout.Button("Reset Key Bindings to Defaults"))
        {
            KeyBindings.ResetToDefaults();
        }

        EditorGUILayout.EndScrollView();

        if (pendingRebind.HasValue && Event.current.type == EventType.KeyDown)
        {
            KeyCode pressed = Event.current.keyCode;
            if (pressed == KeyCode.Escape)
            {
                pendingRebind = null;
                Repaint();
                Event.current.Use();
            }
            else if (pressed != KeyCode.None
                     && pressed != KeyCode.LeftControl && pressed != KeyCode.RightControl
                     && pressed != KeyCode.LeftShift && pressed != KeyCode.RightShift
                     && pressed != KeyCode.LeftAlt && pressed != KeyCode.RightAlt)
            {
                KeyBindings.SetBinding(pendingRebind.Value, pressed);
                pendingRebind = null;
                Repaint();
                Event.current.Use();
            }
        }
    }

    private void DrawKeyBindingRow(KeyBindings.Action action)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(KeyBindings.ActionDisplayName(action), GUILayout.Width(150));

        bool isRebinding = pendingRebind == action;

        if (isRebinding)
        {
            GUI.color = Color.yellow;
            if (GUILayout.Button("Press a key...", GUILayout.Width(120)))
            {
                pendingRebind = null;
                Repaint();
            }
            GUI.color = Color.white;
        }
        else
        {
            KeyCode currentKey = KeyBindings.GetBinding(action);

            bool hasConflict = bindableActions.Any(a => a != action && KeyBindings.GetBinding(a) == currentKey);

            if (hasConflict)
                GUI.color = Color.red;

            if (GUILayout.Button(currentKey.ToString(), GUILayout.Width(120)))
            {
                pendingRebind = action;
                Repaint();
            }

            GUI.color = Color.white;
        }

        EditorGUILayout.EndHorizontal();
    }
}
