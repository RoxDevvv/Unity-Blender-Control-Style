using UnityEditor;
using UnityEngine;

public abstract class BlenderTransformMode {
    /// <summary>
    /// Returns true if the event should trigger the edit mode
    /// </summary>
    public abstract bool ShouldTrigger(Event evt);

    /// <summary>
    /// Initializes this edit mode, should only be called if ShouldTrigger returns true
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Cancels this edit mode, reverting changes
    /// </summary>
    public abstract void Cancel();
    /// <summary>
    /// Applies this edit mode, applying the changes to objects
    /// </summary>
    public abstract void Apply();

    /// <summary>
    /// Processes this edit mode, transforming objects
    /// </summary>
    public abstract void Process(SceneView sv);

    /// <summary>
    /// Called when BlenderManager.CurrentAxis changes
    /// </summary>
    public abstract void OnAxisChange();
    /// <summary>
    /// Called when BlenderManager.CurrentAxisMode changes
    /// </summary>
    public abstract void OnAxisModeChange();
    
    /// <summary>
    /// Called to draw the scene GUI overlay (lines, handles, etc)
    /// </summary>
    public abstract void DrawSceneGUI(SceneView sceneView);
}