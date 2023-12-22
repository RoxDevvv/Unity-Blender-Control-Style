using System;
using UnityEngine;

public static class TransformModeManager
{
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

}
