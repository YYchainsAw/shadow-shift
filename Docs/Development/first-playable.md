# First playable level

The current layout starts in the upper left and leads through the blue wall on the middle landing to the green fan switch and upper-right landing. The lower platforms hold the purple wall, purple switch, and cracked floor. The battery marker is below the cracked floor. [Level layout](../Design/first-playable-greybox.drawio.svg)

## Run the test

1. Open [ShadowShift.unity](../../Assets/Scenes/ShadowShift.unity) in Unity 6000.3.10f1.
2. Set the Game view to **16:9**, enter Play mode, and focus the Game view.
3. Use the controls below. The HUD shows the session, form, and movement states, plus the stability countdown outside the light.

The default build scene list starts with `ShadowShift`.

| Input | Action |
| --- | --- |
| `A` / `D` | Move left or right, including while operating the spotlight. |
| `W` | Jump on a new press from the ground. |
| `Left Shift` | Change form. Enter Shadow while lit. A change succeeds when the destination form fits. |
| Hold left mouse | Drag the lamp horizontally along its rail. It moves relative to the drag's starting position. |
| Hold right mouse | Aim the beam toward the cursor. The beam keeps its aim after release. |
| `Escape` | Pause or resume. Release held mouse buttons before starting another gesture after resuming. |
| `R` | Restore the character to spawn in Physical form, reset the lamp to its starting position and aim, and turn the fan off. Release held mouse buttons before starting another gesture after restart. |

## Playable route

1. Run to the right edge of the starting platform, then jump right to the middle landing. Aim at the character's feet, enter Shadow, and pass through the blue wall. Return to Physical when clear of the wall and step on the green switch to start the fan.
2. Move the lamp over the fan and aim down the wind column. Walk off the middle landing into the wind and enter Shadow while lit. Keep the character's feet lit during the ascent, then move right and return to Physical to land on the blue section of the upper platform.
3. To test completion, reach the `Next Room Entry` trigger at the right end of the bottom floor. It completes the playtest immediately. Press `R` to reset the scene.

## Current behavior and demo design

| Scene element | Current behavior | Planned behavior |
| --- | --- | --- |
| Lower purple switch and upper blue platform | The purple switch is a visual marker. The upper blue platform is active from the start and supports Physical. | Shadow activates the purple switch to enable the upper route. |
| Descent to the lower platforms | Both forms use the same fall gravity multiplier. | Shadow descends more slowly for the approach to the lower switch. |
| Purple wall and cracked floor | The purple wall blocks Shadow. The cracked floor is solid for both forms. Light coverage depends on the beam cone. | The purple wall and intact cracked floor occlude the beam. A sufficiently hard Physical landing breaks the floor and opens a path for the light. |
| Battery and next room | The battery is a visual marker. The bottom-right trigger completes the current playtest on contact. | Light reaching the battery through the opening charges it. A full battery powers the next room's lamp and enables the lamp control handoff. |

## Scene authoring

Under `Level Session > Environment > Geometry`, use `Platforms` for the ledges, `Form Obstacles` for the colored walls and blue platform section, `Fan Assembly` for the working switch and wind, and `Layout Mechanisms` for the three markers. `Level Session > Level Markers` contains spawn, finish, and fall hazard. Use the Level Session component to wire references and adjust the horizontal camera bounds. The temporary character in `ShadowShift` is 3.5 world units tall.

`LevelSession` connects `PhysicalSwitch` to `ShadowUpdraft` and supplies the spotlight to the player's updraft check. `SpotlightInput` controls `SpotlightRail`, which checks the player's foot position against the cone. `FormObstacle` defines which form collides with the blue wall, purple wall, and upper blue platform.

## Acceptance checks

| Test | Expected result |
| --- | --- |
| Jump from the starting ledge and press `W` again before landing. | The character can steer in the air and cannot jump again until grounded. |
| Hold left mouse while walking and drag past both rail ends. | The character keeps moving. The lamp starts from its current position, moves horizontally, and stops at the rail ends. |
| Aim with right mouse, then release and move the cursor. | The cone pivots at the lamp while the button is held and keeps its aim on release. |
| Walk into the middle blue wall as Physical. Bring the light to the character, press `Left Shift`, walk inside, and try switching back. | Physical is blocked by the wall. Shadow passes through it. The shift back is rejected while the character overlaps the wall. |
| Approach the purple wall as Shadow and switch to Physical before crossing. | The purple wall blocks Shadow and lets Physical pass. |
| Stand in Shadow and turn or move the beam away from the character's feet. | The timer starts and steering stops while gravity continues. Returning the beam before the one-second grace period expires restores control. Expiry restarts the test. |
| Pause while airborne or outside the beam as Shadow, then resume. | Position and the countdown freeze while paused. The character resumes its motion with the same vertical velocity. |
| Step off the outside edge of the bottom floor. | The failure cue appears and the level restarts. |
| Step onto the green switch as Shadow, then as Physical. Enter the active wind column in Physical form. | The switch responds to Physical and stays on after the character steps off. Physical receives no lift. |
| Enter the powered wind as lit Shadow. | The wind lifts Shadow while the character's feet remain in the beam. |
| Reach the blue section of the upper platform after the ascent. | Physical can land on the platform. Shadow passes through it. |
| Reach the finish trigger on the bottom floor and press `R`. | The trigger completes the level. Restart returns the character to spawn, resets the lamp, and turns the fan off. A failure also resets the fan. |

Record whether players can move, jump, and aim comfortably at the same time, and whether they can predict when the character will leave the cone.
