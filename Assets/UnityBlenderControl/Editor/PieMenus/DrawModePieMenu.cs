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

    public static void Trigger(ShortcutStage stage) {
        overlay.Perform(new ShortcutArguments { stage = stage, context = SceneView.lastActiveSceneView });
    }

    static PieMenuEntry CreateEntry(string name, string icon, DrawCameraMode mode) {
        return new PieMenuEntry(name, icon,
            () => SceneView.lastActiveSceneView.cameraMode = SceneView.GetBuiltinCameraMode(mode),
            () => SceneView.lastActiveSceneView.cameraMode == SceneView.GetBuiltinCameraMode(mode)
        );
    }
}
