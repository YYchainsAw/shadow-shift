# SpotlightRail integration contract

## Standalone test

Create `Assets/Scenes/SpotlightControlTest.unity` on a feature branch such as `feature/spotlight-control-test`. Use a 2D orthographic camera aligned with world XY, a horizontal rail, and a placeholder light. The test scene demonstrates pointer control and end stops. The main prototype will add the gameplay light footprint, navigation, shadow stability, and level lifecycle.

## Scene setup

| Inspector reference | Setup |
| --- | --- |
| `View Camera` | The camera that displays the test scene and defines how the pointer maps into the world. |
| `Left Endpoint` and `Right Endpoint` | Stationary transforms with the same world Y and Z and distinct world X coordinates. Keep both markers outside the moving light hierarchy. |
| `SpotlightRail` host | Attach the component to the moving light root. Place the root at the starting position on the rail and attach the placeholder visual as a child. |

## Behavior and output

1. Read the current mouse position in screen space through the project's Input System. Pointer movement alone drives the light.
2. Project the pointer onto the rail's world XY plane at the authored Z. Clamp the projected world X between the endpoint X values and use the rail's fixed world Y and Z.
3. Move the light root to that position. If a pointer sample is unavailable, retain the last valid position.

Expose **`Vector3 WorldPosition { get; }`** as the current world-space position of the light root after clamping. It is the shared anchor for the light visual, gameplay footprint, and route target. Keeping the visual as a child lets the integrator swap the placeholder while preserving the anchor.

## Main prototype handoff

- **Light footprint.** Build the gameplay light region from the updated `WorldPosition`. Draw its visible boundary using the same footprint. The footprint owns the point-in-light check used by shadow stability and route eligibility.
- **Route planner.** Use that same position snapshot and footprint to resolve the nearest reachable lit waypoint for the active form. [Locomotion state machine](UML/character-locomotion-state-machine.drawio.svg)
- **Level session.** Gate pointer updates when gameplay is paused, failed, completed, or resetting. Restore the authored initial light position and update the footprint before locomotion samples the reset state. [Level session state machine](UML/level-session-state-machine.drawio.svg)

## Acceptance checks

| Check | Expected result |
| --- | --- |
| Open the test scene and enter Play mode. Move the mouse left and right. | The placeholder follows the pointer along the rail without a click. |
| Move the pointer vertically at a fixed screen X. | The light stays at the same world position. |
| Move the pointer beyond each rail endpoint. | The light stops at the corresponding endpoint. Its world Y and Z stay fixed. |
| Reposition the endpoints along X and run the test again. | The new endpoint positions define the allowed range. |
| Read `WorldPosition` after a pointer update. | It matches the light root's current world position, including when clamped to an endpoint. |

Include the scene and script with their `.meta` files. Share the branch name and a short Game view recording or playtest note for integration review.
