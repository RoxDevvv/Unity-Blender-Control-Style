using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderRotateEditor : Editor
{
    private Vector3 initialRotation;
    private Vector3 selectedAxis;
    private Vector2 mouseStartPosition;

    public void ObjectRotate()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.R
        && CurrentTransformMode != TransformMode.Rotate)
        {
            // Record the initial state for undo
            Undo.RegisterCompleteObjectUndo((Transform)target, "Rotate Object");
            // Activate scale mode when "R" key is pressed
            CurrentTransformMode = TransformMode.Rotate;
            initialRotation = ((Transform)target).localEulerAngles;
            mouseStartPosition = e.mousePosition;
            selectedAxis = Vector3.one;

            ObjectAxis = Vector3.one;
        }


        if (CurrentTransformMode == TransformMode.Rotate)
        {
            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                IntialObjectPosition = ((Transform)target).position;
                ((Transform)target).localRotation = Quaternion.Euler(initialRotation);
                ObjectAxis = BlenderHelper.GetObjectAxis((Transform)target, selectedAxis);
                WorldAxis = selectedAxis;
            }

            // Calculate the center of the object in screen space
            Vector3 objectCenter = HandleUtility.WorldToGUIPoint(((Transform)target).position);
            SelectedObject = (Transform)target;
            // Calculate the initial angle between the object center and the initial mouse position
            float initialAngle = AngleBetweenVector2(objectCenter, mouseStartPosition);

            // Calculate the current angle between the object center and the current mouse position
            float currentAngle = AngleBetweenVector2(objectCenter, e.mousePosition);

            // Calculate the rotation angle based on the difference between initial and current angles
            float rotationAngle = currentAngle - initialAngle;

            // Use a Quaternion to represent the rotation
            Quaternion rotationDelta = Quaternion.AngleAxis(rotationAngle, ObjectAxis);

            // Apply rotation to the object
            ((Transform)target).localRotation = rotationDelta * Quaternion.Euler(initialRotation);

            if (BlenderHelper.CancelKeyPressed(e))
            {
                // Deactivate scale mode and apply changes on Enter or left mouse button click

                CurrentTransformMode = TransformMode.None;
            }

            if (BlenderHelper.RevertKeyPressed(e))
            {
                // Revert changes and deactivate scale mode on right mouse button
                ((Transform)target).localRotation = Quaternion.Euler(initialRotation);

                CurrentTransformMode = TransformMode.None;
            }

            // invoke draw axis function
            DrawAxis?.Invoke();
        }
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
