using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;
public class BlenderMove : BlenderTransformMode {
    struct PerObjectData {
        public Transform Transform;
        public Vector3 InitialPosition;
        public Vector3 InitialMouse;
        public Vector3 InitialOffset => InitialPosition - InitialMouse;
        public Vector3 LocalAxis;
    }

    private List<PerObjectData> perObjectData;
    public Vector3 averagePosition;

    public override bool ShouldTrigger(Event evt) {
        return BlenderHelper.ShouldTriggerSimple(evt, KeyCode.G);
    }

    public override void Initialize() {
        var transforms = Selection.transforms;
        Undo.RegisterCompleteObjectUndo(transforms, "Rotate Object");
        perObjectData = new List<PerObjectData>();
        averagePosition = Vector3.zero;
        foreach (var transform in transforms) {
            perObjectData.Add(new PerObjectData {
                Transform = transform,
                InitialPosition = transform.position,
                InitialMouse = GetWorldMouse(transform.position),
                LocalAxis = BlenderHelper.GetObjectAxis(transform, BlenderManager.CurrentAxisVector)
            });
            averagePosition += transform.position;
        }
        averagePosition /= transforms.Length;
    }

    public override void Cancel() {
        foreach (var data in perObjectData) {
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
                MoveByUnit(data);
            } else {
                MoveByMouse(data);
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
        switch (BlenderManager.CurrentAxisMode) {
            case BlenderManager.AxisMode.Unlocked:
                break;
            case BlenderManager.AxisMode.Global:
                // draw at average position
                BlenderManager.DrawAxisLine(averagePosition, BlenderManager.CurrentAxisVector);
                break;
            case BlenderManager.AxisMode.Local:
                // draw at each object's position
                foreach (var data in perObjectData) {
                    BlenderManager.DrawAxisLine(data.Transform.position, data.LocalAxis);
                }
                break;
        }
    }

    Vector3 GetWorldMouse(Vector3 pos) {
        Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
        float distance_to_screen = sceneViewCamera.WorldToScreenPoint(pos).z;
        // Invert the Y-axis
        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = sceneViewCamera.pixelHeight - mousePosition.y;

        return sceneViewCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance_to_screen));
    }
    private void MoveByUnit(PerObjectData data) {
        var pos = BlenderManager.CurrentNumber * (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked ? data.Transform.right : GetAxis(data));
        data.Transform.position = pos + data.InitialPosition;
    }

    private Vector3 GetAxis(PerObjectData data) {
        return BlenderManager.CurrentAxisMode switch {
            BlenderManager.AxisMode.Local => data.LocalAxis,
            BlenderManager.AxisMode.Global => BlenderManager.CurrentAxisVector,
            _ => Vector3.zero
        };
    }
    
    private void MoveByMouse(PerObjectData data) {
        Vector3 currentMousePosition = GetWorldMouse(data.InitialPosition);
        Vector3 snapValue = BlenderHelper.GetSnapMove();
        var target = data.Transform;
        if (BlenderManager.CurrentAxisMode == BlenderManager.AxisMode.Unlocked)
        {
            if (isSnappingEnabled)
            {
                Vector3 direction = currentMousePosition + data.InitialOffset - data.InitialPosition;
                target.position = data.InitialPosition + SnapPosition(direction, snapValue);
            }
            else
            {
                target.position = currentMousePosition + data.InitialOffset;
            }
        }
        else
        {
            var axis = GetAxis(data);
            // Calculate the distance along the object axis
            float distance = Vector3.Dot(axis, currentMousePosition - data.InitialPosition + data.InitialOffset);

            if (isSnappingEnabled)
            {
                // Snap the distance based on snapValue
                float snappedDistance = Mathf.Round(distance / snapValue.magnitude) * snapValue.magnitude;
                // Update the object's position
                target.position = data.InitialPosition + (axis * snappedDistance);
            }
            else
            {
                // Calculate the direction from initial to current position
                Vector3 direction = data.InitialMouse - currentMousePosition;

                //Debug.DrawLine(initialMouse, currentMousePosition);

                // Update the target position
                target.position = data.InitialPosition + (distance* direction.magnitude * axis);
            }

        }
    }
    Vector3 SnapPosition(Vector3 position, Vector3 snapValue)
    {
        // Snap position to grid based on snapValue
        Vector3 snappedPosition;
        snappedPosition.x = Mathf.Round(position.x / snapValue.x) * snapValue.x;
        snappedPosition.y = Mathf.Round(position.y / snapValue.y) * snapValue.y;
        snappedPosition.z = Mathf.Round(position.z / snapValue.z) * snapValue.z;
        return snappedPosition;
    }
}
