using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderRotate : BlenderTransformMode {
    struct PerObjectData {
        public Transform Transform;
        public Vector3 InitialPosition;
        public Quaternion InitialRotation;
        public Vector3 LocalAxis;
    }

    private List<PerObjectData> perObjectData;

    private Vector2 mouseStartPosition;
    public Vector3 averagePosition;
    Bounds bounds;

    public override bool ShouldTrigger(Event evt) {
        return BlenderHelper.ShouldTriggerSimple(evt, KeyCode.R);
    }

    public override void Initialize() {
        var transforms = Selection.transforms;
        Undo.RegisterCompleteObjectUndo(transforms, "Rotate Object");
        perObjectData = new List<PerObjectData>();
        mouseStartPosition = Event.current.mousePosition;
        averagePosition = Vector3.zero;
        bounds.SetMinMax(Vector3.positiveInfinity, Vector3.negativeInfinity);
        foreach (var transform in transforms) {
            perObjectData.Add(new PerObjectData {
                Transform = transform,
                InitialPosition = transform.position,
                InitialRotation = transform.rotation,
                LocalAxis = BlenderHelper.GetObjectAxis(transform, BlenderManager.CurrentAxisVector)
            });

            averagePosition += transform.position;

            bounds.Encapsulate(transform.position);
        }
        averagePosition /= transforms.Length;
    }

    public override void Cancel() {
        foreach (var data in perObjectData) {
            data.Transform.position = data.InitialPosition;
            data.Transform.rotation = data.InitialRotation;
        }
        perObjectData = null;
    }

    public override void Apply() {
        perObjectData = null;
    }

    public override void Process(SceneView sv) {
        foreach (var data in perObjectData) {
            if (BlenderManager.MoveByNumber) {
                RotateByAngle(sv, data);
            } else {
                RotateByMouse(sv, data);
            }
        }
    }

    public override void OnAxisChange() {
        // new axis, update local axis for all objects
        for (var i = 0; i < perObjectData.Count; i++) {
            var objectData = perObjectData[i];
            objectData.Transform.rotation = objectData.InitialRotation;
            objectData.LocalAxis = BlenderHelper.GetObjectAxis(objectData.Transform, BlenderManager.CurrentAxisVector);
            perObjectData[i] = objectData;
        }
    }

    public override void OnAxisModeChange() {
        // do nothing
    }

    public override void DrawSceneGUI(SceneView sceneView) {
        float screenScale = Screen.dpi / 96f;
        // draw a black line between the mouse and the pivot point (average object position)
        var mp = Event.current.mousePosition;
        // for some reason the mouse and ScreenToWorldPoint use opposite y axies, so flip that around by doing viewport height - y
        var mouseWorldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(screenScale * mp.x, screenScale * (sceneView.cameraViewport.height - mp.y), 1));
        Vector3 center = BlenderHelper.GetTransformationCenter(averagePosition, bounds);
        Handles.color = Color.black;
        Handles.DrawLine(center, mouseWorldPos);

        // TODO refactor duplicated code
        if (BlenderManager.LocationOnly && BlenderManager.CurrentPivotPoint == BlenderManager.PivotPoint.IndividualOrigins) {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.NotAllowed);
            // Trigger repaint because the line connecting the center and cursor wouldn't be repainted otherwise when only the mouse position changes.
            SceneView.RepaintAll();
            return;
        }
        else {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ResizeUpRight);
        }

        // TODO eliminate nearly identical code
        switch (BlenderManager.CurrentAxisMode) {
            case BlenderManager.AxisMode.Unlocked:
                break;
            case BlenderManager.AxisMode.Global:
                BlenderManager.DrawAxisLine(center, BlenderManager.CurrentAxisVector, true);
                break;
            case BlenderManager.AxisMode.Local:
                foreach (var data in perObjectData) {
                    BlenderManager.DrawAxisLine(data.InitialPosition, data.LocalAxis, Selection.activeTransform == data.Transform);
                }
                break;
        }
    }

    private void DoRotate(SceneView sv, PerObjectData data, float amount) {
        Quaternion deltaRotation = BlenderManager.CurrentAxisMode switch {
            BlenderManager.AxisMode.Local => Quaternion.AngleAxis(amount, BlenderManager.CurrentAxisVector),
            BlenderManager.AxisMode.Global => Quaternion.AngleAxis(amount, BlenderManager.CurrentAxisVector),
            BlenderManager.AxisMode.Unlocked => Quaternion.AngleAxis(amount, -sv.camera.transform.forward),
            _ => Quaternion.identity
        };

        // Rotation change
        if (!BlenderManager.LocationOnly) {
            if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Local) {
                data.Transform.rotation = data.InitialRotation * deltaRotation;
            }
            else {
                data.Transform.rotation = deltaRotation * data.InitialRotation;
            }
        }

        // Position change
        if (BlenderManager.CurrentPivotPoint != BlenderManager.PivotPoint.IndividualOrigins) {
            Vector3 center = BlenderHelper.GetTransformationCenter(averagePosition, bounds);
            Vector3 globalDir = data.InitialPosition - center;
            Vector3 rotatedGlobalDir;
            if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Local) {
                Vector3 localDir = Quaternion.Inverse(data.InitialRotation) * globalDir;
                Vector3 rotatedLocalDir = deltaRotation * localDir;
                rotatedGlobalDir = data.InitialRotation * rotatedLocalDir;
            }
            else {
                rotatedGlobalDir = deltaRotation * globalDir;
            }
            data.Transform.position = center + rotatedGlobalDir;
        }
    }

    void RotateByMouse(SceneView sv, PerObjectData data) {
        float snapValue = BlenderHelper.GetSnapRotate();
        // Calculate the center of the object in screen space
        //Vector3 objectCenter = HandleUtility.WorldToGUIPoint(data.Transform.position);
        Vector3 center = HandleUtility.WorldToGUIPoint(BlenderHelper.GetTransformationCenter(averagePosition, bounds));
        // Calculate the initial angle between the object center and the initial mouse position
        float initialAngle = AngleBetweenVector2(center, mouseStartPosition);

        // Calculate the current angle between the object center and the current mouse position
        float currentAngle = AngleBetweenVector2(center, Event.current.mousePosition);

        // Calculate the rotation angle based on the difference between initial and current angles
        float rotationAngle = initialAngle - currentAngle;

        // calculate snap rotation
        float snapRotation = Mathf.Round(rotationAngle / snapValue) * snapValue;

        // When looking from the opposite direction the rotation needs to be inverted
        if (SceneView.lastActiveSceneView != null) {
            Vector3 viewDirection = SceneView.lastActiveSceneView.camera.transform.forward;
            if ((BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Local
                && Vector3.Dot(viewDirection, Selection.activeTransform.rotation * BlenderManager.CurrentAxisVector) > 0f)
                || (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Global
                && Vector3.Dot(viewDirection, BlenderManager.CurrentAxisVector) > 0f)) {
                rotationAngle = -rotationAngle;
            }
        }

        // Use a Quaternion to represent the rotation
        float angle = isSnappingEnabled ? snapRotation : rotationAngle;

        DoRotate(sv, data, angle);
    }

    void RotateByAngle(SceneView sv, PerObjectData data) {
        DoRotate(sv, data, BlenderManager.CurrentNumber);
    }

    // Function to calculate the angle between two Vector2 points
    float AngleBetweenVector2(Vector3 vec1, Vector3 vec2) {
        Vector3 from = vec2 - vec1;
        Vector3 to = new Vector3(1, 0, 0); // You can change this to your desired reference vector

        float angle = Vector3.SignedAngle(from, to, Vector3.forward);

        return angle;
    }
}
