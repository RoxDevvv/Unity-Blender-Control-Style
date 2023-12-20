using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderScaleEditor : Editor
{
    private Vector3 initialScale;
    private Vector3 selectedAxis;
    private Vector2 mouseStartPosition;

    public void ObjectScale()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.S
        && CurrentTransformMode != TransformMode.Scale)
        {
            // Activate scale mode when "S" key is pressed
            CurrentTransformMode = TransformMode.Scale;
            initialScale = ((Transform)target).localScale;
            mouseStartPosition = e.mousePosition;
            selectedAxis = Vector3.one;

            ObjectAxis = Vector3.zero;
        }


        if (CurrentTransformMode == TransformMode.Scale)
        {
            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                IntialObjectPosition = ((Transform)target).position;
            }

            ObjectAxis = BlenderHelper.GetObjectAxis((Transform)target, selectedAxis);
            WorldAxis = selectedAxis;

            // Calculate the center of the object in screen space
            Vector3 objectCenter = HandleUtility.WorldToGUIPoint(((Transform)target).position);
            SelectedObject = (Transform)target;
            // Calculate the initial distance between the object center and the initial mouse position
            float initialLineLength = Vector2.Distance(objectCenter, mouseStartPosition);

            // Calculate the current distance between the object center and the current mouse position
            float currentLineLength = Vector2.Distance(objectCenter, e.mousePosition);

            // Calculate the scale factor based on the ratio of initial and current line lengths
            float scaleFactor = 1f + (currentLineLength - initialLineLength) * 0.01f;

            // Apply scale to the object
            //((Transform)target).localScale = initialScale * Mathf.Clamp(scaleFactor, 0.1f, 10f);
            ((Transform)target).localScale = new Vector3(
                initialScale.x * ModifyScaleVector(scaleFactor, selectedAxis).x,
                initialScale.y * ModifyScaleVector(scaleFactor, selectedAxis).y,
                initialScale.z * ModifyScaleVector(scaleFactor, selectedAxis).z
            );

            if (BlenderHelper.CancelKeyPressed(e))
            {
                // Deactivate scale mode and apply changes on Enter or left mouse button click

                CurrentTransformMode = TransformMode.None;
            }

            if (BlenderHelper.RevertKeyPressed(e))
            {
                // Revert changes and deactivate scale mode on right mouse button
                ((Transform)target).localScale = initialScale;

                CurrentTransformMode = TransformMode.None;
            }

            // invoke draw axis function
            DrawAxis?.Invoke();
        }
    }

    Vector3 ModifyScaleVector(float scaleFactor, Vector3 axis)
    {
        return new Vector3(
            axis.x == 0 ? 1f : scaleFactor,
            axis.y == 0 ? 1f : scaleFactor,
            axis.z == 0 ? 1f : scaleFactor
        );
    }
}
