using UnityEditor;

[InitializeOnLoad]
public static class TransformModeManager
{
    static TransformModeManager() {
        // load settings on startup / domain reload
        LoadSettings();
    }
    
    internal static void LoadSettings() {
        isBlenderPluginEnabled = EditorPrefs.GetBool("BlenderControlPluginEnabled", true);
        swapYAndZ = EditorPrefs.GetBool("BlenderControlPluginSwapYAndZ", false);
        
    }
    
    internal static void SaveSettings()
    {
        EditorPrefs.SetBool("BlenderControlPluginEnabled", isBlenderPluginEnabled);
        EditorPrefs.SetBool("BlenderControlPluginSwapYAndZ", swapYAndZ);
    }

    public static bool isBlenderPluginEnabled = true;
    public static bool isSnappingEnabled = false;
    public static bool swapYAndZ = false;
}