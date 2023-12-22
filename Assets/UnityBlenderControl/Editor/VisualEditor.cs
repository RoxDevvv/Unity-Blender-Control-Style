using UnityEditor;
using UnityEngine;
using static TransformModeManager;


[InitializeOnLoad]
public class ChangeMouseCursorInEditor
{
    static ChangeMouseCursorInEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        DrawAxis += CreateAxis;
    }
    private static void CreateAxis()
    {
        if (SelectedObject != null)
        {
            DrawAxisLine();
        }
    }
    static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();

        // Clamp cursor position to stay within Scene view bounds
        Vector2 cursorPos = Event.current.mousePosition;

        if (CurrentTransformMode == TransformMode.Scale || CurrentTransformMode == TransformMode.Rotate)
        {

            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ResizeUpRight);

            if (SelectedObject != null)
            {
                // Calculate the center of the object in screen space
                Vector3 objectCenter = HandleUtility.WorldToGUIPoint(SelectedObject.position);

                // Draw a 2D line from the object center to the initial mouse position
                Handles.color = Color.black;
                Handles.DrawLine(objectCenter, cursorPos);
            }

        }

        Handles.EndGUI();
    }
    static void DrawAxisLine()
    {
        if (ObjectAxis == Vector3.one || ObjectAxis == Vector3.zero)
            return;

        Color lineColor;

        if (WorldAxis == Vector3.right)
        {
            lineColor = Color.red;
        }
        else if (WorldAxis == Vector3.up)
        {
            lineColor = Color.green;
        }
        else if (WorldAxis == Vector3.forward)
        {
            lineColor = Color.blue;
        }
        else
        {
            lineColor = Color.black;
        }

        Handles.color = lineColor;
        Vector3 StartPoint = IntialObjectPosition - ObjectAxis * 1000f;
        Vector3 endPoint = IntialObjectPosition + ObjectAxis * 1000f;


        Handles.DrawLine(StartPoint, endPoint);
    }
}
