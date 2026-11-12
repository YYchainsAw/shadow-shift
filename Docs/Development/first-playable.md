# First playable level

The first playable level has a jump block, two Physical Solids, one Shadow Barrier, and a fan ascent to the upper exit.

## Run the test

1. Open [ShadowShift.unity](../../Assets/Scenes/ShadowShift.unity) in Unity 6000.3.10f1.
2. Set the Game view to **16:9**, enter Play mode, and focus the Game view.
3. Use the controls below. The HUD shows the session, form, and movement states, plus the stability countdown outside the light.

The default Windows build starts in the same scene.

| Input | Action |
| --- | --- |
| `A` / `D` | Move left or right, including while operating the spotlight. |
| `W` | Jump on a new press from the ground. |
| `Left Shift` | Change form. Enter Shadow while lit. A change succeeds when the destination form fits. |
| Hold left mouse | Drag the lamp horizontally along its rail. It moves relative to the drag's starting position. |
| Hold right mouse | Aim the beam toward the cursor. The beam keeps its aim after release. |
| `Escape` | Pause or resume. Release held mouse buttons before starting another gesture after resuming. |
| `R` | Restore the character, Physical form, lamp position, and downward aim, and turn the fan off. Release held mouse buttons before starting another gesture after restart. |

## Fan ascent

1. Cross the wide brown wall as Shadow while keeping the character's feet in the light. Return to Physical on the other side and walk onto the green switch to start the fan.
2. Hold the left mouse button and drag the lamp to the right rail endpoint. Hold the right mouse button and aim at the marker in the wind column.
3. Stop inside the lit wind column and switch to Shadow. Stay in the beam during the ascent. At the upper landing, switch back to Physical to land, then walk left to the exit.

## Scene authoring

Use the Level Session component to wire the player, spawn point, spotlight, exit, hazard, form obstacles, fan switch, and updraft. Set each Form Obstacle type in the Inspector. Move the two rail endpoints and the initial lamp pose to tune the crossing. Adjust the camera bounds on Level Session to frame the level.

## Acceptance checks

| Test | Expected result |
| --- | --- |
| Move right, jump over the grey block, and press `W` again before landing. | The grey block stops both forms. The character can steer in the air, and each jump requires a fresh press while grounded. |
| Hold left mouse while walking and drag past both rail ends. | The character keeps moving. The lamp starts from its current position, moves horizontally, and stops at the rail ends. |
| Aim with right mouse, then release and move the cursor. | The cone pivots at the lamp while the button is held and keeps its aim on release. |
| Walk into the first brown wall as Physical. Bring the light to the character, press `Left Shift`, walk inside, and try switching back. | Physical is blocked by the wall. Shadow passes through it. The shift back is rejected while the character overlaps the wall. |
| Approach the purple wall as Shadow and switch to Physical before crossing. | The purple wall blocks Shadow and lets Physical pass. |
| Stand in Shadow and turn or move the beam away from the character's feet. | The timer starts and steering stops while gravity continues. Returning the beam before the one-second grace period expires restores control. Expiry restarts the test. |
| Pause while airborne or outside the beam as Shadow, then resume. | Position and the countdown freeze while paused. The character resumes its motion with the same vertical velocity. |
| Walk left from spawn into the red hazard. | The failure cue appears and the level restarts. |
| Keep the beam over the character while crossing the wide brown wall. | Shadow clears the wall and can return to Physical on the far side. |
| Step onto the green switch as Shadow, then as Physical. Walk through the active wind column in Physical form. | The switch responds to Physical and stays on after the character steps off. Physical walks through at ground level. |
| Try the ascent with the lamp aimed straight down, then aim it at the wind marker. | The downward beam loses coverage before the top. With the beam aimed at the marker, lit Shadow rises to the landing. |
| At the top, return to Physical and walk left into the exit. Press `R` after completion. | The exit completes the level. Restart returns the character to spawn, resets the lamp, and turns the fan off. A failure also resets the fan. |

The rendered cone and the foot-position check use the same dimensions. With the lamp at `y = 6.8`, the nine-unit cone with a 52-degree opening covers about 6.6 units at ground level when aimed down. Move the lamp as you cross the eight-unit wall. At the right rail endpoint (`x = 26.2`), aim toward the wind marker to cover the ascent.

Record whether players can move, jump, and aim comfortably at the same time, and whether they can predict when the character will leave the cone.
