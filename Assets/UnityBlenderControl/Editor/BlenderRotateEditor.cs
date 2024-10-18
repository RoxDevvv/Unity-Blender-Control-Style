using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderRotateEditor : Editor
{
    private Vector3 initialRotation;
    private Vector3 selectedAxis;
    private Vector2 mouseStartPosition;
    private string RotationNumber = "";
    public void ObjectRotate()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.R
        && CurrentTransformMode != TransformMode.Rotate
        && !BlenderHelper.RightMouseHeld)
        {
            // Record the initial state for undo
            Undo.RegisterCompleteObjectUndo((Transform)target, "Rotate Object");
            // Activate scale mode when "R" key is pressed
            CurrentTransformMode = TransformMode.Rotate;
            initialRotation = ((Transform)target).eulerAngles;
            mouseStartPosition = e.mousePosition;
            selectedAxis = Vector3.one;

            ObjectAxis = Vector3.one;
            RotationNumber = "";
        }


        if (CurrentTransformMode == TransformMode.Rotate)
        {

            BlenderHelper.AppendUnitNumber(e, ref RotationNumber);

            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                IntialObjectPosition = ((Transform)target).position;
                ((Transform)target).rotation = Quaternion.Euler(initialRotation);
                ObjectAxis = Tools.pivotRotation == PivotRotation.Local
                    ? BlenderHelper.GetObjectAxis((Transform)target, selectedAxis)
                    :  selectedAxis;
                WorldAxis = selectedAxis;
            }
            if (!RotateByAngle())
            {
                RotateByMouse(e);
            }

            if (BlenderHelper.CancelKeyPressed(e))
            {
                // Deactivate scale mode and apply changes on Enter or left mouse button click

                CurrentTransformMode = TransformMode.None;
            }

            if (BlenderHelper.RevertKeyPressed(e))
            {
                // Revert changes and deactivate scale mode on right mouse button
                ((Transform)target).rotation = Quaternion.Euler(initialRotation);

                CurrentTransformMode = TransformMode.None;
            }

            // invoke draw axis function
            DrawAxis?.Invoke();
        }
    }

    void RotateByMouse(Event e)
    {
        float snapValue = BlenderHelper.GetSnapRotate();
        // Calculate the center of the object in screen space
        Vector3 objectCenter = HandleUtility.WorldToGUIPoint(((Transform)target).position);
        SelectedObject = (Transform)target;
        // Calculate the initial angle between the object center and the initial mouse position
        float initialAngle = AngleBetweenVector2(objectCenter, mouseStartPosition);

        // Calculate the current angle between the object center and the current mouse position
        float currentAngle = AngleBetweenVector2(objectCenter, e.mousePosition);

        // Calculate the rotation angle based on the difference between initial and current angles
        float rotationAngle = currentAngle - initialAngle;

        // calculate snap rotation
        float SnapRotation = Mathf.Round(rotationAngle / snapValue) * snapValue;
        
        // Use a Quaternion to represent the rotation
        float Angle = isSnappingEnabled ? SnapRotation : rotationAngle;

        Quaternion rotationDelta = Quaternion.AngleAxis(Angle, ObjectAxis);
        // Apply rotation to the object
        ((Transform)target).rotation = rotationDelta * Quaternion.Euler(initialRotation);
    }
    bool RotateByAngle()
    {
        // Parse the rotation angle string
        if (float.TryParse(RotationNumber, out float angle))
        {
            // Rotate based on the angle input
            Quaternion rotationDelta = Quaternion.AngleAxis(angle, ObjectAxis);
            ((Transform)target).localRotation = rotationDelta * Quaternion.Euler(initialRotation);
            return true;
        }
        return false;
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
