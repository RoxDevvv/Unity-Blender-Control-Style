using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderScale : BlenderTransformMode {
    struct PerObjectData {
        public Transform Transform;
        public Vector3 InitialPosition;
        public Vector3 InitialScale;
        public Vector3 LocalAxis;
    }

    private List<PerObjectData> perObjectData;

    private Vector2 mouseStartPosition;
    public Vector3 averagePosition;
    Bounds bounds;

    public override bool ShouldTrigger(Event evt) {
        return BlenderHelper.ShouldTriggerSimple(evt, KeyCode.S);
    }

    public override void Initialize() {
        var transforms = Selection.transforms;
        Undo.RegisterCompleteObjectUndo(transforms, "Rotate Object");
        perObjectData = new List<PerObjectData>();
        averagePosition = Vector3.zero;
        mouseStartPosition = Event.current.mousePosition;
        bounds.SetMinMax(Vector3.positiveInfinity, Vector3.negativeInfinity);
        foreach (var transform in transforms) {
            perObjectData.Add(new PerObjectData {
                Transform = transform,
                InitialPosition = transform.position,
                InitialScale = transform.localScale,
                LocalAxis = BlenderHelper.GetObjectAxis(transform, BlenderManager.CurrentAxisVector)
            });

            averagePosition += transform.position;

            bounds.Encapsulate(transform.position);
        }
        averagePosition /= transforms.Length;
    }

    public override void Cancel() {
        foreach (var data in perObjectData) {
            data.Transform.localScale = data.InitialScale;
            data.Transform.position = data.InitialPosition;
        }
        perObjectData = null;
    }

    public override void Apply() {
        perObjectData = null;
    }

    public override void Process(SceneView sv) {
        foreach (var data in perObjectData) {
            if (BlenderManager.MoveByNumber) {
                ScaleByUnit(data);
            } else {
                ScaleByMouse(data);
            }
        }
    }

    public override void OnAxisChange() {
        // new axis, update local axis for all objects
        for (var i = 0; i < perObjectData.Count; i++) {
            var objectData = perObjectData[i];
            objectData.LocalAxis = BlenderHelper.GetObjectAxis(objectData.Transform, BlenderManager.CurrentAxisVector);
            perObjectData[i] = objectData;
        }
    }

    public override void OnAxisModeChange() {
        // do nothing
    }

    public override void DrawSceneGUI(SceneView sceneView) {
        // change mouse icon
        EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ResizeUpRight);

        float screenScale = Screen.dpi / 96f;
        // draw a black line between the mouse and the pivot point (average object position)
        var mp = Event.current.mousePosition;
        // for some reason the mouse and ScreenToWorldPoint use opposite y axies, so flip that around by doing viewport height - y
        var mouseWorldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(screenScale * mp.x, screenScale * (sceneView.cameraViewport.height - mp.y), 1));
        Vector3 center = BlenderHelper.GetTransformationCenter(averagePosition, bounds);
        Handles.color = Color.black;
        Handles.DrawLine(center, mouseWorldPos);

        // TODO eliminate nearly identical code
        switch (BlenderManager.CurrentAxisMode) {
            case BlenderManager.AxisMode.Unlocked:
                break;
            case BlenderManager.AxisMode.Global:
                BlenderManager.DrawAxisLine(center, BlenderManager.CurrentAxisVector);
                break;
            case BlenderManager.AxisMode.Local:
                foreach (var data in perObjectData) {
                    BlenderManager.DrawAxisLine(data.InitialPosition, data.LocalAxis);
                }
                break;
        }
    }

    void DoScale(PerObjectData data, float amount) {
        // Scale change
        Vector3 scale;
        if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked) {
            scale = amount * Vector3.one;
            data.Transform.localScale = Vector3.Scale(scale, data.InitialScale);
        }
        else {
            scale = Vector3.one + BlenderManager.CurrentAxisVector * (amount - 1f);
            if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Global) {
                data.Transform.localScale = Quaternion.Inverse(data.Transform.rotation) * Vector3.Scale(scale, data.Transform.rotation * data.InitialScale);
            }
            else { // BlenderManager.AxisMode.Local
                data.Transform.localScale = Vector3.Scale(scale, data.InitialScale);
            }
        }

        // Postion change
        if (BlenderManager.CurrentPivotPoint != BlenderManager.PivotPoint.IndividualOrigins) {
            Vector3 center = BlenderHelper.GetTransformationCenter(averagePosition, bounds);
            if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked) {
                data.Transform.position = center + (data.InitialPosition - center) * amount;
            }
            else {
                Vector3 projectionAxis = BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Global
                    ? BlenderManager.CurrentAxisVector
                    : data.Transform.rotation * BlenderManager.CurrentAxisVector;
                float initialDistance = -Vector3.Dot(projectionAxis, center - data.InitialPosition);
                Vector3 projectedCenter = data.InitialPosition + projectionAxis * -initialDistance;
                data.Transform.position = projectedCenter + projectionAxis * initialDistance * amount;
            }
        }
    }

    void ScaleByUnit(PerObjectData data) {
        DoScale(data, BlenderManager.CurrentNumber);
    }

    void ScaleByMouse(PerObjectData data) {
        float snapValue = BlenderHelper.GetSnapScale();
        // Calculate the center of the object in screen space
        var center = HandleUtility.WorldToGUIPoint(BlenderHelper.GetTransformationCenter(averagePosition, bounds));

        Vector3 centerToStartMouse = mouseStartPosition - center;
        Vector3 centerToCurrentMouse = Event.current.mousePosition - center;

        // Calculate the scale factor based on the ratio of initial and current line lengths
        float initialLineLength = centerToStartMouse.magnitude;
        float currentLineLength = centerToCurrentMouse.magnitude;
        float scaleFactor = currentLineLength / initialLineLength;
        if (Vector3.Dot(centerToStartMouse, centerToCurrentMouse) < 0f) {
            scaleFactor = -scaleFactor;
        }
        // calculate snap scale
        float SnapScale = Mathf.Round(scaleFactor / snapValue) * snapValue;
        SnapScale = SnapScale == 0 ? 1f : SnapScale;

        float DesiredScale = isSnappingEnabled ? SnapScale : scaleFactor;
        // Apply scale to the object
        DoScale(data, DesiredScale);
    }
}
