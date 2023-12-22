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
        isBlenderPluginEnabled = EditorPrefs.GetBool("IsPluginEnabled", true);
    }
    private void OnGUI()
    {
        GUILayout.Label("Blender Plugin Control", EditorStyles.boldLabel);

        isBlenderPluginEnabled = EditorGUILayout.Toggle("Enable Plugin", isBlenderPluginEnabled);

        if (GUILayout.Button("Apply"))
        {
            ApplySettings();
        }
    }

    private void ApplySettings()
    {
        // Save the setting
        EditorPrefs.SetBool("IsPluginEnabled", isBlenderPluginEnabled);
        // Implement logic to enable or disable your plugin based on the 'isPluginEnabled' variable.
        if (isBlenderPluginEnabled)
        {
            Debug.Log("Plugin Enabled");
            // Enable your plugin
        }
        else
        {
            Debug.Log("Plugin Disabled");
            // Disable your plugin
        }
    }
}
