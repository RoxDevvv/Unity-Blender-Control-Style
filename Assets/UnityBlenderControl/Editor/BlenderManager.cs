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

    public enum Axis {
        None,
        X,
        Y,
        Z,
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
    public static bool LocationOnly = false;
    public static Axis CurrentAxis = Axis.None;

    public static AxisMode CurrentAxisMode {
        get {
            if (LockToAxis) {
                return Tools.pivotRotation == PivotRotation.Global ? AxisMode.Global : AxisMode.Local;
            } else {
                return AxisMode.Unlocked;
            }
        }
    }
    public static Vector3 CurrentAxisVector {
        get {
            return CurrentAxis switch {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                Axis.Z => Vector3.forward,
                _ => Vector3.one
            };
        }
    }
    private static string CurrentNumberString = "";
    private static bool CurrentNumberIsPositive = true;
    public static float CurrentNumber = 0;
    public static bool MoveByNumber => !float.IsNaN(CurrentNumber);

    private static void Reset() {
        CurrentTransformMode = null;
        CurrentAxis = Axis.None;
        CurrentNumberString = "";
        CurrentNumberIsPositive = true;
        ResetAxisLockMode();
    }

    static void AdvanceAxisLockMode() {
        // change axis mode Unlocked -> Global -> Local -> Unlocked
        if (!LockToAxis) {
            LockToAxis = true;
        }
        else {
            if (Tools.pivotRotation == PreviousPivotRotation) {
                // switch pivot rotation
                Tools.pivotRotation = Tools.pivotRotation == PivotRotation.Global ? PivotRotation.Local : PivotRotation.Global;
            }
            else {
                // revert to unlocked
                LockToAxis = false;
                Tools.pivotRotation = PreviousPivotRotation;
            }
        }
    }

    static void ResetAxisLockMode() {
        LockToAxis = false;
        Tools.pivotRotation = PreviousPivotRotation;
    }

    static Color GetAxisColor(bool active) {
        // The active axis is slightly brighter (controlled with c)
        float c = active ? 0.6f : 0f;
        return CurrentAxis switch {
            Axis.X => new Color(1f, c, c),
            Axis.Y => new Color(c, 1f, c),
            Axis.Z => new Color(c, c, 1f),
            _ => Color.white
        };
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

        if (Event.current.type == EventType.KeyDown && !(Event.current.alt || Event.current.control)) {
            Axis newAxis = Event.current.keyCode switch {
                KeyCode.X => Axis.X,
                KeyCode.Y => swapYAndZ ? Axis.Z : Axis.Y,
                KeyCode.Z => swapYAndZ ? Axis.Y : Axis.Z,
                _ => Axis.None
            };
            // OnlyOtherAxis = Event.current.shift;
            if (newAxis != Axis.None) {
                if (CurrentAxis == Axis.None || newAxis == CurrentAxis) {
                    AdvanceAxisLockMode();
                }
                else {
                    ResetAxisLockMode();
                }
                CurrentAxis = newAxis;
                CurrentTransformMode.OnAxisChange();
                Event.current.Use();
                return;
            }
        }

        BlenderHelper.AppendUnitNumber(Event.current, ref CurrentNumberString, ref CurrentNumberIsPositive);

        if (BlenderHelper.TryParseUnitNumber(CurrentNumberString, CurrentNumberIsPositive, out var newNumber)) {
            CurrentNumber = newNumber;
        }
        else {
            CurrentNumber = float.NaN;
        }

        CurrentTransformMode.Process(sv);

        if (BlenderHelper.RevertKeyPressed(Event.current)) {
            CurrentTransformMode.Cancel();
            Reset();
        }
        else if (BlenderHelper.ApplyKeyPressed(Event.current)) {
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

    public static void DrawAxisLine(Vector3 origin, Vector3 direction, bool active) {
        if (direction == Vector3.one || direction == Vector3.zero)
            return;

        Handles.color = GetAxisColor(active);
        var startPoint = origin - direction * 1000f;
        var endPoint = origin + direction * 1000f;
        Handles.DrawLine(startPoint, endPoint);
    }
}
