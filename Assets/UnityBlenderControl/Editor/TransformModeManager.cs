using System;
using UnityEditor;
using UnityEngine;

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
    
    public enum TransformMode
    {
        None,
        Move,
        Rotate,
        Scale
    }

    public static TransformMode CurrentTransformMode { get; set; }
    public static Transform SelectedObject { get; set; }
    public static Vector3 IntialObjectPosition { get; set; }
    public static Vector3 ObjectAxis { get; set; }
    public static Vector3 WorldAxis { get; set; }
    public static Action DrawAxis;
    public static bool isBlenderPluginEnabled = true;
    public static bool isSnappingEnabled = false;
    public static bool swapYAndZ = false;
}