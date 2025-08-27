using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderRotate : BlenderTransformMode
{
    struct PerObjectData {
        public Transform Transform;
        public Vector3 InitialPosition;
        public Quaternion InitialRotation;
        public Vector3 LocalAxis;
    }
    
    private List<PerObjectData> perObjectData;
    
    private Vector2 mouseStartPosition;
    public Vector3 averagePosition;

    public override bool ShouldTrigger(Event evt) {
        var targets = Selection.transforms;
        
        return BlenderHelper.IsKeyDown(evt, KeyCode.R)
            && !BlenderHelper.IsModifierPressed(evt)
            && !BlenderHelper.RightMouseHeld
            && targets.Length > 0;
    }

    public override void Initialize() {
        var transforms = Selection.transforms;
        Undo.RegisterCompleteObjectUndo(transforms, "Rotate Object");
        perObjectData = new List<PerObjectData>();
        mouseStartPosition = Event.current.mousePosition;
        averagePosition = Vector3.zero;
        foreach (var transform in transforms) {
            perObjectData.Add(new PerObjectData {
                Transform = transform,
                InitialPosition = transform.position,
                InitialRotation = transform.rotation,
                LocalAxis = BlenderHelper.GetObjectAxis(transform, BlenderManager.CurrentAxisVector)
            });
            averagePosition += transform.position;
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
        // change mouse icon
        EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ResizeUpRight);

        float screenScale = Screen.dpi / 96f;
        // draw a black line between the mouse and the pivot point (average object position)
        var mp = Event.current.mousePosition;
        // for some reason the mouse and ScreenToWorldPoint use opposite y axies, so flip that around by doing viewport height - y
        var mouseWorldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(screenScale * mp.x, screenScale * (sceneView.cameraViewport.height - mp.y), 1));
        Handles.color = Color.black;
        Handles.DrawLine(averagePosition, mouseWorldPos);
        
        if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked) {
            return;
        }
        
        // draw at each object's position
        foreach (var data in perObjectData) {
            var direction = BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Global
                ? BlenderManager.CurrentAxisVector
                : data.LocalAxis;
            BlenderManager.DrawAxisLine(data.Transform.position, direction);
        }
    }

    private void DoRotate(SceneView sv, PerObjectData data, float amount) {
        switch (BlenderManager.CurrentAxisMode) {
            case BlenderManager.AxisMode.Local:
                data.Transform.rotation = data.InitialRotation * Quaternion.AngleAxis(amount, BlenderManager.CurrentAxisVector);
                break;
            case BlenderManager.AxisMode.Global:
                data.Transform.rotation = Quaternion.AngleAxis(amount, BlenderManager.CurrentAxisVector) * data.InitialRotation;
                break;
            case BlenderManager.AxisMode.Unlocked:
                data.Transform.rotation = Quaternion.AngleAxis(amount, -sv.camera.transform.forward) * data.InitialRotation;
                break;
            default:
                data.Transform.rotation = data.InitialRotation;
                break;
        }
    }

    void RotateByMouse(SceneView sv, PerObjectData data)
    {
        float snapValue = BlenderHelper.GetSnapRotate();
        // Calculate the center of the object in screen space
        //Vector3 objectCenter = HandleUtility.WorldToGUIPoint(data.Transform.position);
        Vector3 center = HandleUtility.WorldToGUIPoint(averagePosition);
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
                && Vector3.Dot(viewDirection, data.LocalAxis) > 0f)
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
    float AngleBetweenVector2(Vector3 vec1, Vector3 vec2)
    {
        Vector3 from = vec2 - vec1;
        Vector3 to = new Vector3(1, 0, 0); // You can change this to your desired reference vector

        float angle = Vector3.SignedAngle(from, to, Vector3.forward);

        return angle;
    }
}
