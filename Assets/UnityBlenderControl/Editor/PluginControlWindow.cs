using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class PluginControlWindow : EditorWindow
{
    [MenuItem("Blender/Blender Control Window")]
    public static void ShowWindow()
    {
        GetWindow<PluginControlWindow>("Blender Plugin Control");
    }
    private void OnEnable()
    {
        // Load the saved setting when the window is enabled
        LoadSettings();
    }
    private void OnGUI()
    {
        GUILayout.Label("Blender Plugin Control", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();

        isBlenderPluginEnabled = EditorGUILayout.Toggle("Enable Plugin", isBlenderPluginEnabled);
        swapYAndZ = EditorGUILayout.Toggle("Swap Y and Z", swapYAndZ);

        if (EditorGUI.EndChangeCheck()) {
            // save settings if one of them was changed
            SaveSettings();
        }
    }
}