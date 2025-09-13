using JonasWischeropp.Unity.EditorTools.SceneView;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class DrawModePieMenu {
    static PieMenu overlay = new PieMenu(new PieMenuEntry[]{
        CreateEntry("Shaded", "TreeEditor.Material", DrawCameraMode.Normal),
        CreateEntry("Wireframe", "TreeEditor.Geometry On", DrawCameraMode.Wireframe),
        CreateEntry("Shaded Wireframe", "d_PreMatSphere", DrawCameraMode.TexturedWire),
    });

    [ClutchShortcut("Blender Controls/Draw Mode Pie Menu", typeof(SceneView), KeyCode.Z)]
    static void PerformPieMenu(ShortcutArguments arguments) {
        overlay.Perform(arguments);
    }

    static PieMenuEntry CreateEntry(string name, string icon, DrawCameraMode mode) {
        return new PieMenuEntry(name, icon,
            () => SceneView.lastActiveSceneView.cameraMode = SceneView.GetBuiltinCameraMode(mode),
            () => SceneView.lastActiveSceneView.cameraMode == SceneView.GetBuiltinCameraMode(mode)
        );
    }
}
