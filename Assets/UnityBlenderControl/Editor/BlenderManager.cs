using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;

[InitializeOnLoad]
public static class BlenderManager {
    public enum AxisMode {
        Unlocked = 0,
        Global = 1,
        Local = 2
    }

    public enum PivotPoint {
        IndividualOrigins,
        BoundingBoxCenter,
        ActiveElement,
        MedianPoint,
    }

    static BlenderManager() {
        // Create an instance of BlenderMove when the BlenderManager is enabled
        TransformModes = new List<BlenderTransformMode> { new BlenderMove(), new BlenderRotate(), new BlenderScale() };

        SceneView.duringSceneGui -= OnDuringSceneGUI;
        SceneView.duringSceneGui += OnDuringSceneGUI;
    }

    public static List<BlenderTransformMode> TransformModes;
    public static BlenderTransformMode CurrentTransformMode;


    private static bool LockToAxis = false;
    private static PivotRotation PreviousPivotRotation = PivotRotation.Global;
    public static PivotPoint CurrentPivotPoint = PivotPoint.MedianPoint;

    public static AxisMode CurrentAxisMode {
        get {
            if (LockToAxis) {
                return Tools.pivotRotation == PivotRotation.Global ? AxisMode.Global : AxisMode.Local;
            } else {
                return AxisMode.Unlocked;
            }
        }
    }
    public static Vector3 CurrentAxisVector = Vector3.zero;
    public static Color CurrentAxisColor = Color.white;
    private static string CurrentNumberString = "";
    private static bool CurrentNumberIsPositive = true;
    public static float CurrentNumber = 0;
    public static bool MoveByNumber => !float.IsNaN(CurrentNumber);

    private static void Reset() {
        CurrentTransformMode = null;
        CurrentAxisVector = Vector3.zero;
        CurrentNumberString = "";
        CurrentNumberIsPositive = true;
        // reset AxisMode
        LockToAxis = false;
        Tools.pivotRotation = PreviousPivotRotation;
    }

    private static void OnDuringSceneGUI(SceneView sv) {
        if (!isBlenderPluginEnabled)
            return;

        BlenderHelper.RightMouseHeldCheck();
        BlenderHelper.CheckSnap();

        foreach (var transformMode in TransformModes) {
            if (transformMode != CurrentTransformMode && transformMode.ShouldTrigger(Event.current)) {
                CurrentTransformMode?.Cancel();
                Reset();
                CurrentTransformMode = transformMode;
                CurrentTransformMode.Initialize();
            }
        }

        if (CurrentTransformMode == null) {
            PreviousPivotRotation = Tools.pivotRotation;
            return;
        }

        var axisCode = BlenderHelper.AxisKeycode(Event.current);
        if (axisCode != KeyCode.None) {
            var newAxisVector = BlenderHelper.GetAxisVector(axisCode);
            if (newAxisVector == CurrentAxisVector) {
                // change axis mode Unlocked -> Global -> Local -> Unlocked
                if (!LockToAxis) {
                    LockToAxis = true;
                } else {
                    if (Tools.pivotRotation == PreviousPivotRotation) {
                        // switch pivot rotation
                        Tools.pivotRotation = Tools.pivotRotation == PivotRotation.Global ? PivotRotation.Local : PivotRotation.Global;
                    } else {
                        // revert to unlocked
                        LockToAxis = false;
                        Tools.pivotRotation = PreviousPivotRotation;
                    }
                }

                CurrentTransformMode.OnAxisModeChange();
            } else {
                LockToAxis = true;
                Tools.pivotRotation = PreviousPivotRotation;

                CurrentAxisColor = BlenderHelper.GetAxisColor(axisCode);
                CurrentAxisVector = newAxisVector;
                CurrentTransformMode.OnAxisChange();
            }
        }

        BlenderHelper.AppendUnitNumber(Event.current, ref CurrentNumberString, ref CurrentNumberIsPositive);

        if (BlenderHelper.TryParseUnitNumber(CurrentNumberString, CurrentNumberIsPositive, out var newNumber)) {
            CurrentNumber = newNumber;
        } else {
            CurrentNumber = float.NaN;
        }

        CurrentTransformMode.Process(sv);

        if (BlenderHelper.RevertKeyPressed(Event.current)) {
            CurrentTransformMode.Cancel();
            Reset();
        } else if (BlenderHelper.ApplyKeyPressed(Event.current)) {
            CurrentTransformMode.Apply();
            Reset();
        }

        if (Event.current.type == EventType.Repaint) {
            CurrentTransformMode?.DrawSceneGUI(sv);
        }
    }

    public static Vector3 GetWorldAxisVector(Vector3 localVector) {
        return CurrentAxisMode switch {
            AxisMode.Unlocked => Vector3.zero,
            AxisMode.Global => CurrentAxisVector,
            AxisMode.Local => localVector,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static void DrawAxisLine(Vector3 origin, Vector3 direction)
    {
        if (direction == Vector3.one || direction == Vector3.zero)
            return;

        Handles.color = CurrentAxisColor;
        var startPoint = origin - direction * 1000f;
        var endPoint = origin + direction * 1000f;
        Handles.DrawLine(startPoint, endPoint);
    }
}
