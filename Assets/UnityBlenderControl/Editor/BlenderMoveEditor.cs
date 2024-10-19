
using UnityEditor;
using UnityEngine;
using static TransformModeManager;
public class BlenderMoveEditor : Editor
{
    private Vector3 initialPosition;
    private Vector3 selectedAxis;

    Vector3 initialOffset;
    private string moveNumber = "";
    private bool moveNumberIsPositive = true;
    private Vector3 initialMouse;
    public void ObjectMove()
    {
        Event e = Event.current;
        Transform targetObj = (Transform)target;

        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.G
        && !BlenderHelper.IsModifierPressed(e)
        && CurrentTransformMode != TransformMode.Move
        && !BlenderHelper.RightMouseHeld)
        {
            // Record the initial state for undo
            Undo.RegisterCompleteObjectUndo((Transform)target, "Move Object");
            // Activate Move mode when "G" key is pressed
            CurrentTransformMode = TransformMode.Move;
            initialPosition = targetObj.position;

            selectedAxis = Vector3.one;

            // Calculate the initial offset
            initialOffset = targetObj.position - GetWorldMouse(((Transform)target).position);
            initialMouse = GetWorldMouse(((Transform)target).position);

            ObjectAxis = Vector3.zero;
            moveNumber = "";
            moveNumberIsPositive = true;
        }


        if (CurrentTransformMode == TransformMode.Move)
        {

            BlenderHelper.AppendUnitNumber(e, ref moveNumber, ref moveNumberIsPositive);

            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                targetObj.position = initialPosition;
                initialOffset = initialPosition - GetWorldMouse(((Transform)target).position);
                IntialObjectPosition = initialPosition;

                ObjectAxis = Tools.pivotRotation == PivotRotation.Local
                    ? BlenderHelper.GetObjectAxis((Transform)target, selectedAxis)
                    :  selectedAxis;
                WorldAxis = selectedAxis;
            }


            SelectedObject = targetObj;

            if (!MoveByUnit(targetObj))
            {
                moveByMouse(targetObj);
            }

            if (BlenderHelper.CancelKeyPressed(e))
            {
                // Deactivate move mode and apply changes on Enter or left mouse button click
                CurrentTransformMode = TransformMode.None;
            }

            if (BlenderHelper.RevertKeyPressed(e))
            {
                // Revert changes and deactivate move mode on right mouse button
                targetObj.position = initialPosition;
                CurrentTransformMode = TransformMode.None;
            }

            // invoke draw axis function
            DrawAxis?.Invoke();
        }
    }

    Vector3 GetWorldMouse(Vector3 pos)
    {
        Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
        float distance_to_screen = sceneViewCamera.WorldToScreenPoint(pos).z;
        // Invert the Y-axis
        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = sceneViewCamera.pixelHeight - mousePosition.y;

        return sceneViewCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance_to_screen));
    }
    bool MoveByUnit(Transform target)
    {
        // Parse the move unit string
        if (BlenderHelper.TryParseUnitNumber(moveNumber, moveNumberIsPositive, out float MoveUnit))
        {
            Vector3 pos = MoveUnit * (ObjectAxis == Vector3.zero ? target.right : ObjectAxis);
            // move based on the unit input
            target.position = pos + initialPosition;
            return true;
        }
        return false;
    }
    //BlenderHelper.GetSnapMove()
    void moveByMouse(Transform target)
    {
        Vector3 currentMousePosition = GetWorldMouse(initialPosition);
        Vector3 snapValue = BlenderHelper.GetSnapMove();
        if (ObjectAxis == Vector3.zero)
        {
            if (isSnappingEnabled)
            {
                Vector3 direction = currentMousePosition + initialOffset - initialPosition;
                target.position = initialPosition + SnapPosition(direction, snapValue);
            }
            else
            {
                target.position = currentMousePosition + initialOffset;
            }
        }
        else
        {
            // Calculate the distance along the object axis
            float distance = Vector3.Dot(ObjectAxis, currentMousePosition - initialPosition + initialOffset);

            if (isSnappingEnabled)
            {
                // Snap the distance based on snapValue
                float snappedDistance = Mathf.Round(distance / snapValue.magnitude) * snapValue.magnitude;
                // Update the object's position
                target.position = initialPosition + (ObjectAxis * snappedDistance);
            }
            else
            {
                // Calculate the direction from initial to current position
                Vector3 direction = initialMouse - currentMousePosition;

                //Debug.DrawLine(initialMouse, currentMousePosition);

                // Update the target position
                target.position = initialPosition + (distance* direction.magnitude * ObjectAxis);
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
