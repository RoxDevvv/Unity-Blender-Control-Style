using JonasWischeropp.Unity.EditorTools.SceneView;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using PivotPoint = BlenderManager.PivotPoint;

public static class PivotPointPieMenu {
    static PieMenu overlay = new PieMenu(new PieMenuEntry[]{
        // TODO icons
        CreateEntry("Individual Origins", "TreeEditor.Material", PivotPoint.IndividualOrigins),
        new PieMenuEntry("Only Location", "TreeEditor.Material", () => BlenderManager.LocationOnly = !BlenderManager.LocationOnly, () => BlenderManager.LocationOnly),
        CreateEntry("Bounding Box Center", "TreeEditor.Material", PivotPoint.BoundingBoxCenter),
        CreateEntry("Active Element", "TreeEditor.Material", PivotPoint.ActiveElement),
        CreateEntry("Media Point", "TreeEditor.Material", PivotPoint.MedianPoint),
    });

    [ClutchShortcut("Blender Controls/Pivot Point Pie Menu", typeof(SceneView), KeyCode.Period)]
    static void PerformPieMenu(ShortcutArguments arguments) {
        if (BlenderManager.CurrentTransformMode == null) {
            overlay.Perform(arguments);
        }
    }

    static PieMenuEntry CreateEntry(string name, string icon, PivotPoint mode) {
        return new PieMenuEntry(name, icon, () => BlenderManager.CurrentPivotPoint = mode, () => BlenderManager.CurrentPivotPoint == mode);
    }
}
