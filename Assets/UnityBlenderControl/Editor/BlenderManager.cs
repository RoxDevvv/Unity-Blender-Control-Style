using System;
using UnityEditor;
using UnityEngine;
using static TransformModeManager;
[CustomEditor(typeof(Transform), true)]
public class BlenderManager : Editor
{
    private Editor defaultEditor;
    private BlenderMoveEditor blenderMoveInstance;
    private BlenderScaleEditor blenderScaleInstance;
    private BlenderRotateEditor blenderRotateInstance;

    private void OnEnable()
    {
        // Create an instance of BlenderMove when the BlenderManager is enabled
        blenderMoveInstance = (BlenderMoveEditor)CreateEditor(target, typeof(BlenderMoveEditor));
        blenderScaleInstance = (BlenderScaleEditor)CreateEditor(target, typeof(BlenderScaleEditor));
        blenderRotateInstance = (BlenderRotateEditor)CreateEditor(target, typeof(BlenderRotateEditor));
        // Find the default TransformInspector and create an instance of it
        Type transformInspectorType = Type.GetType("UnityEditor.TransformInspector, UnityEditor");
        if (transformInspectorType != null)
        {
            defaultEditor = Editor.CreateEditor(targets, transformInspectorType);
        }
    }
    public override void OnInspectorGUI()
    {
        // Draw the default Transform inspector
        if (defaultEditor != null)
        {
            defaultEditor.OnInspectorGUI();
        }
    }
    public void OnSceneGUI()
    {
        if (!isBlenderPluginEnabled)
            return;
        if (blenderMoveInstance != null)
        {
            blenderMoveInstance.ObjectMove();
        }
        if (blenderScaleInstance != null)
        {
            blenderScaleInstance.ObjectScale();
        }
        if (blenderRotateInstance != null)
        {
            blenderRotateInstance.ObjectRotate();
        }
    }
}
