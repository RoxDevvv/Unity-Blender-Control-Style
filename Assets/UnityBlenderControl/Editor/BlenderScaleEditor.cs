using UnityEditor;
using UnityEngine;
using static TransformModeManager;

public class BlenderScaleEditor : Editor
{
    private Vector3 initialScale;
    private Vector3 selectedAxis;
    private Vector2 mouseStartPosition;
    private string ScaleNumber = "";
    private bool scaleNumberIsPositive = true;
    public void ObjectScale()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown
        && e.keyCode == KeyCode.S
        && CurrentTransformMode != TransformMode.Scale
        && !BlenderHelper.RightMouseHeld)
        {
            // Record the initial state for undo
            Undo.RegisterCompleteObjectUndo((Transform)target, "Scale Object");
            // Activate scale mode when "S" key is pressed
            CurrentTransformMode = TransformMode.Scale;
            initialScale = ((Transform)target).localScale;
            mouseStartPosition = e.mousePosition;
            selectedAxis = Vector3.one;

            ObjectAxis = Vector3.zero;
            ScaleNumber = "";
        }


        if (CurrentTransformMode == TransformMode.Scale)
        {

            BlenderHelper.AppendUnitNumber(e, ref ScaleNumber, ref scaleNumberIsPositive);

            KeyCode AxisCode = BlenderHelper.AxisKeycode(e);
            if (AxisCode != KeyCode.None)
            {
                selectedAxis = BlenderHelper.GetAxisVector(AxisCode);
                IntialObjectPosition = ((Transform)target).position;
            }

            ObjectAxis = BlenderHelper.GetObjectAxis((Transform)target, selectedAxis);
            WorldAxis = selectedAxis;
            if (!ScaleByUnit())
            {
                ScaleByMouse(e);
            }

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
    bool ScaleByUnit()
    {
        // Parse the move unit string
        if (BlenderHelper.TryParseUnitNumber(ScaleNumber, scaleNumberIsPositive, out float scaleUnit))
        {
            Vector3 scale = ModifyScaleVector(scaleUnit, selectedAxis);
            ((Transform)target).localScale = Vector3.Scale(scale, initialScale);
            return true;
        }
        return false;
    }
    void ScaleByMouse(Event e)
    {
        float snapValue = BlenderHelper.GetSnapScale();
        // Calculate the center of the object in screen space
        Vector3 objectCenter = HandleUtility.WorldToGUIPoint(((Transform)target).position);
        SelectedObject = (Transform)target;
        // Calculate the initial distance between the object center and the initial mouse position
        float initialLineLength = Vector2.Distance(objectCenter, mouseStartPosition);

        // Calculate the current distance between the object center and the current mouse position
        float currentLineLength = Vector2.Distance(objectCenter, e.mousePosition);

        // Calculate the scale factor based on the ratio of initial and current line lengths
        float scaleFactor = 1f + (currentLineLength - initialLineLength) * 0.01f;
        // calculate snap scale
        float SnapScale = Mathf.Round(scaleFactor / snapValue) * snapValue;
        SnapScale = SnapScale == 0 ? 1f : SnapScale;
        
        float DesiredScale = isSnappingEnabled ? SnapScale : scaleFactor;
        // Apply scale to the object
        Vector3 scale = ModifyScaleVector(DesiredScale, selectedAxis);
        ((Transform)target).localScale = Vector3.Scale(scale, initialScale); ;
    }
}
