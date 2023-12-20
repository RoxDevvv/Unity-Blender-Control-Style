
using UnityEditor;
using UnityEngine;
using static TransformModeManager;
public class BlenderMoveEditor : Editor
{
    private Vector3 initialPosition;
    private Vector3 selectedAxis;

    Vector3 initialOffset;
    public void ObjectMove()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.G
        && CurrentTransformMode != TransformMode.Move)
        {

            // Activate Move mode when "G" key is pressed
            CurrentTransformMode = TransformMode.Move;
            initialPosition = ((Transform)target).position;

            selectedAxis = Vector3.one;

            // Calculate the initial offset
            initialOffset = ((Transform)target).position - GetWorldMouse();

            
            ObjectAxis = Vector3.zero;
        }


        if (CurrentTransformMode == TransformMode.Move)
        {
            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                ((Transform)target).position = initialPosition;
                initialOffset = initialPosition - GetWorldMouse();
                IntialObjectPosition = initialPosition;

                ObjectAxis = BlenderHelper.GetObjectAxis((Transform)target, selectedAxis);
                WorldAxis = selectedAxis;
            }


            SelectedObject = (Transform)target;


            var WorldMouse = GetWorldMouse();
 
            ((Transform)target).position = new Vector3(
               selectedAxis.x == 0 ? initialPosition.x : WorldMouse.x + initialOffset.x,
               selectedAxis.y == 0 ? initialPosition.y : WorldMouse.y + initialOffset.y,
               selectedAxis.z == 0 ? initialPosition.z : WorldMouse.z + initialOffset.z
            );

            if (BlenderHelper.CancelKeyPressed(e))
            {
                // Deactivate move mode and apply changes on Enter or left mouse button click
                CurrentTransformMode = TransformMode.None;
            }

            if (BlenderHelper.RevertKeyPressed(e))
            {
                // Revert changes and deactivate move mode on right mouse button
                ((Transform)target).position = initialPosition;
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

}
