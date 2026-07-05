## [0.1.0] - Unreleased

### Added
- Pivot point modes: Individual Origins, Median Point, Bounding Box Center, Active Element
- Location-only transformation mode
- Draw Mode pie menu (Shaded / Wireframe / Shaded Wireframe) via `D` key
- Pivot Point pie menu via `.` (Period) key
- Axis indicator lines for Rotate and Scale modes
- Cursor icon changes during Rotate and Scale
- Y/Z axis swap option in settings
- Settings autosave via EditorPrefs
- Floating point and backspace support in numeric input
- Negative scale support
- Key binding system with rebindable controls via control window

### Changed
- Transform modes now hook into `SceneView.duringSceneGui` instead of custom editor — fixes keybinding conflicts
- Axis mode cycles through Unlocked → Global → Local via repeated axis key presses
- Axis mode now uses Unity's `Tools.pivotRotation` and auto-reverts on exit
- Rotation direction adapts based on view direction
- Numeric input defaults to positive numbers
- Consolidated key binding configuration into a grouped layout

### Fixed
- Rotation direction inverted depending on camera view
- Rotation not working when mouse hasn't moved
- Scaling with pivot point miscalculations
- Position change calculation during rotation
- Line-to-cursor rendering on different monitor scales
- Key events not being consumed properly
- Overlay appearing when pressing `S` (Scale)
- Rotation by numeric angle
- Cancel method on BlenderScale not fully reverting
- Exception when entering numeric input without prior digits
- Modifier keys (`Ctrl`/`Alt`/`Shift`) incorrectly triggering transforms

## [0.0.1] - 22 December 2023
- Initial Version