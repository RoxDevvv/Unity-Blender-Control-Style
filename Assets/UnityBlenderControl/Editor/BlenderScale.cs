using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderScale : BlenderTransformMode
{
        struct PerObjectData {
        public Transform Transform;
        public Vector3 InitialPosition;
        public Vector3 InitialScale;
        public Vector3 LocalAxis;
    }
    
    private List<PerObjectData> perObjectData;
    
    private Vector2 mouseStartPosition;
    public Vector3 averagePosition;

    public override bool ShouldTrigger(Event evt) {
        var targets = Selection.transforms;

        return evt.type == EventType.KeyDown
               && evt.keyCode == KeyCode.S
               && !BlenderHelper.IsModifierPressed(evt)
               && !BlenderHelper.RightMouseHeld
               && targets.Length > 0;
    }

    public override void Initialize() {
        var transforms = Selection.transforms;
        Undo.RegisterCompleteObjectUndo(transforms, "Rotate Object");
        perObjectData = new List<PerObjectData>();
        averagePosition = Vector3.zero;
        mouseStartPosition = Event.current.mousePosition;
        foreach (var transform in transforms) {
            perObjectData.Add(new PerObjectData {
                Transform = transform,
                InitialPosition = transform.position,
                InitialScale = transform.localScale,
                LocalAxis = BlenderHelper.GetObjectAxis(transform, BlenderManager.CurrentAxisVector)
            });
            averagePosition += transform.position;
        }
        averagePosition /= transforms.Length;
    }

    public override void Cancel() {
        foreach (var data in perObjectData) {
            data.Transform.localScale = data.InitialScale;
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


        // draw a black line between the mouse and the pivot point (average object position)
        var mp = Event.current.mousePosition;
        // for some reason the mouse and ScreenToWorldPoint use opposite y axies, so flip that around by doing viewport height - y
        var mouseWorldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(mp.x, sceneView.cameraViewport.height - mp.y, 1));
        Handles.color = Color.black;
        Handles.DrawLine(averagePosition, mouseWorldPos);

        if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked) {
            return;
        }
        
        foreach (var data in perObjectData) {
            BlenderManager.DrawAxisLine(data.Transform.position, data.LocalAxis);
        }
    }

    Vector3 ModifyScaleVector(float scaleFactor) {
        var axis = BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked
            ? Vector3.one
            : BlenderManager.CurrentAxisVector;
        return new Vector3(
            axis.x == 0 ? 1f : scaleFactor,
            axis.y == 0 ? 1f : scaleFactor,
            axis.z == 0 ? 1f : scaleFactor
        );
    }
    void ScaleByUnit(PerObjectData data)
    {
        Vector3 scale = ModifyScaleVector(BlenderManager.CurrentNumber);
        data.Transform.localScale = Vector3.Scale(scale, data.InitialScale);
    }
    void ScaleByMouse(PerObjectData data)
    {
        float snapValue = BlenderHelper.GetSnapScale();
        // Calculate the center of the object in screen space
        var center = HandleUtility.WorldToGUIPoint(averagePosition);
        // Calculate the initial distance between the object center and the initial mouse position
        float initialLineLength = Vector2.Distance(center, mouseStartPosition);

        // Calculate the current distance between the object center and the current mouse position
        float currentLineLength = Vector2.Distance(center, Event.current.mousePosition);

        // Calculate the scale factor based on the ratio of initial and current line lengths
        float scaleFactor = currentLineLength / initialLineLength;
        // calculate snap scale
        float SnapScale = Mathf.Round(scaleFactor / snapValue) * snapValue;
        SnapScale = SnapScale == 0 ? 1f : SnapScale;
        
        float DesiredScale = isSnappingEnabled ? SnapScale : scaleFactor;
        // Apply scale to the object
        Vector3 scale = ModifyScaleVector(DesiredScale);
        data.Transform.localScale = Vector3.Scale(scale, data.InitialScale);
    }
}
