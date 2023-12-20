using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Transform))]
public class BlenderManager : Editor
{
    private BlenderMoveEditor blenderMoveInstance;
    private BlenderScaleEditor blenderScaleInstance;
    private BlenderRotateEditor blenderRotateInstance;

    private void OnEnable()
    {
        // Create an instance of BlenderMove when the BlenderManager is enabled
        blenderMoveInstance = (BlenderMoveEditor)CreateEditor(target, typeof(BlenderMoveEditor));
        blenderScaleInstance = (BlenderScaleEditor)CreateEditor(target, typeof(BlenderScaleEditor));
        blenderRotateInstance = (BlenderRotateEditor)CreateEditor(target, typeof(BlenderRotateEditor));
    }

    public void OnSceneGUI()
    {
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
