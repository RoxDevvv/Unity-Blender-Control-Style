
using UnityEditor;
using UnityEngine;
using static TransformModeManager;
public class BlenderMoveEditor : Editor
{
    private Vector3 initialPosition;
    private Vector3 selectedAxis;

    Vector3 initialOffset;
    private string moveNumber = "";
    public void ObjectMove()
    {
        Event e = Event.current;
        Transform targetObj = (Transform)target;
        
        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.G
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
            initialOffset = targetObj.position - GetWorldMouse();


            ObjectAxis = Vector3.zero;
            moveNumber = "";
        }


        if (CurrentTransformMode == TransformMode.Move)
        {

            BlenderHelper.AppendUnitNumber(e, ref moveNumber);

            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                targetObj.position = initialPosition;
                initialOffset = initialPosition - GetWorldMouse();
                IntialObjectPosition = initialPosition;


                ObjectAxis = BlenderHelper.GetObjectAxis(targetObj, selectedAxis);
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

    Vector3 GetWorldMouse()
    {
        Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
        float distance_to_screen = sceneViewCamera.WorldToScreenPoint(((Transform)target).position).z;
        // Invert the Y-axis
        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = sceneViewCamera.pixelHeight - mousePosition.y;

        return sceneViewCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance_to_screen));
    }
    bool MoveByUnit(Transform target)
    {
        // Parse the move unit string
        if (float.TryParse(moveNumber, out float MoveUnit))
        {
            Vector3 pos = MoveUnit * (ObjectAxis == Vector3.zero ? target.right : ObjectAxis);
            // move based on the unit input
            target.position = pos + initialPosition;
            return true;
        }
        return false;
    }
    void moveByMouse(Transform target)
    {
        Vector3 currentMousePosition = GetWorldMouse();
        if (ObjectAxis == Vector3.zero)
        {
            target.position = currentMousePosition + initialOffset;
        }
        else
        {
            // Calculate the distance along the object axis
            float distance = Vector3.Dot(ObjectAxis, currentMousePosition - initialPosition + initialOffset);
            // Update the object's position
            target.position = initialPosition + (ObjectAxis * distance);
        }
    }
}
