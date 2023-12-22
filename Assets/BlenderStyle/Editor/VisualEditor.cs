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

        Handles.color = WorldAxis switch
        {
            Vector3 right when right == Vector3.right => Color.red,
            Vector3 up when up == Vector3.up => Color.green,
            Vector3 forward when forward == Vector3.forward => Color.blue,
            _ => Color.black,
        };

        Vector3 StartPoint = IntialObjectPosition - ObjectAxis * 1000f;
        Vector3 endPoint = IntialObjectPosition + ObjectAxis * 1000f;


        Handles.DrawLine(StartPoint, endPoint);
    }
}
