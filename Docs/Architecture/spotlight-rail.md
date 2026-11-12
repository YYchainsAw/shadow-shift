# Spotlight rail setup

## Scene setup

The [Shadow Shift scene](../../Assets/Scenes/ShadowShift.unity) uses a 2D orthographic camera, a lamp, and two rail endpoint transforms.

| Inspector reference | Setup |
| --- | --- |
| `View Camera` | The camera that displays the test scene and defines how the pointer maps into the world. |
| `Left Endpoint` and `Right Endpoint` | Stationary transforms with the same world Y and Z and distinct world X coordinates. Keep both markers outside the moving light hierarchy. |
| `SpotlightRail` host | Attach the component to the moving light root. Place the root at the starting position on the rail. Rotate its beam around the lamp pivot and keep the visible beam and gameplay footprint on the same transform. |
| `Beam Shader` | Assign the [Spotlight Beam shader](../../Assets/Shaders/SpotlightBeam.shader) used to render the cone. |
| `Beam Length` and `Beam Half Angle` | Configure the finite cone dimensions for the greybox and use them for both rendering and illumination checks. |

## Behavior and output

1. Read the current mouse position and button states through the project's Input System. Project valid pointer samples onto the rail's world XY plane at the authored Z.
2. On a left-button press, capture the projected pointer baseline at the lamp's current position. While held, apply the horizontal displacement since the previous valid pointer sample, then advance the baseline. Clamp each displacement to the endpoint X values and keep the rail's fixed world Y and Z. A fresh press starts from the current lamp position.
3. While the right button is held, rotate the beam in world XY so its centerline points from the lamp to the projected pointer. Retain the previous aim when the cursor coincides with the lamp pivot.
4. Process both held buttons in the same gameplay update, applying translation before aiming. Preserve position and rotation when their corresponding buttons are released. Missing pointer samples preserve the last valid transform. Character movement and jumping continue independently while the mouse controls are active.
5. Clear drag baselines on release, focus loss, pause, and reset. Restore the authored lamp position and aim on reset. After a reset or resume, require held mouse buttons to be released before starting a new gesture.

Expose **`Vector3 WorldPosition { get; }`** as the current world-space lamp position after clamping. Share the beam's rotation and configured dimensions with the footprint system. Capture these values together after pointer updates so the visible cone and illumination test use one transform snapshot. Keeping visuals below the lamp pivot lets the integrator replace placeholders while preserving that anchor.

## Gameplay integration

- **Light footprint.** Build the gameplay cone from the updated lamp transform and dimensions. Draw its visible boundary using the same footprint. The footprint owns the foot-position check used by form eligibility and shadow stability. [Light footprint and direct movement](light-footprint-and-movement.md)
- **Character movement.** Read A/D and W independently of drag and aim. Shadow stability gates steering and jumping, and the active form determines collision behavior. [Locomotion state machine](UML/character-locomotion-state-machine.drawio.svg)
- **Level session.** Gate pointer updates when gameplay is paused, failed, completed, or resetting. Restore the authored initial lamp position and aim, then update the footprint before evaluating the reset character. [Level session state machine](UML/level-session-state-machine.drawio.svg)

## Acceptance checks

| Check | Expected result |
| --- | --- |
| Move the cursor with both mouse buttons released. | The lamp keeps its position and aim. |
| Hold the left button and drag horizontally while moving with A/D and jumping with W. | The lamp moves relative to the drag baseline while the character remains controllable. |
| Drag vertically at a fixed projected world X with only the left button held. | The lamp stays at the same world position and keeps its aim. |
| Drag past either rail endpoint, then reverse direction. | The lamp clamps to that endpoint and moves away immediately when the pointer reverses. Its world Y and Z stay fixed. |
| Hold the right button and move the cursor, then release it. | The cone rotates around the lamp toward the cursor and retains the last aim on release. |
| Hold both buttons while moving the cursor. | The lamp translates along the rail and the beam aims from its updated position. |
| Reposition the endpoints along X and run the test again. | The new endpoint positions define the allowed range. |
| Read the lamp position, beam rotation, and dimensions after a pointer update. | They match the rendered cone and the shared gameplay footprint. |
| Restart while a mouse button is held. | The lamp returns to its initial position and aim. Releasing and pressing the button begins a new gesture. |
