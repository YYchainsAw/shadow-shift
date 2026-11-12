# Light footprint and direct movement

This contract connects the [spotlight rail and aiming](spotlight-rail.md) to form switching, shadow stability, and player-controlled locomotion for the [first playable greybox](../Design/first-playable-greybox.drawio.svg). The controls were confirmed on 11 November. Beam dimensions remain configurable greybox values.

## Light footprint

- Build a finite triangular cone in world XY from the lamp position, beam rotation, length, and half angle. The apex is the lamp pivot and the cone rotates with its aim. Use those same values for the rendered triangle and illumination test.
- Transform the sample point into beam-local coordinates. A point is lit when it lies between the apex and configured beam length and its lateral distance is within the cone edge at that depth. Points on the boundary count as lit. For this greybox, walls leave the footprint intact.
- Sample the character at a **foot anchor** shared by both forms. The position markers in the greybox are references for layout and playtest reporting.
- Capture one footprint after lamp translation and aiming in each gameplay update. Form eligibility and shadow stability read that same footprint and foot position.

## Character input and movement

Read A/D as horizontal input, with simultaneous presses producing zero. A new W press requests a jump only while grounded. Consume each jump request once and require another press after landing. A/D continues to steer in the air. Use collision and gravity for movement, the jump apex, and landing. The lamp's controls remain available while the character moves and jumps.

| Movement state | Entry and behavior |
| --- | --- |
| Idle | Grounded with zero horizontal input. |
| Moving | Grounded with horizontal input. Apply the configured horizontal speed, subject to collision. |
| Jumping | A grounded W press applies jump velocity. Keep airborne steering while ascending. |
| Falling | Descending or walking off a ledge. Keep airborne steering and return to Idle or Moving on landing. |

Report blocked movement when collision prevents the requested motion. If Shadow leaves the cone, suppress horizontal steering and jump requests while gravity continues and show `Outside light`. Reevaluate the permitted movement from the current input when illumination returns or the character successfully returns to Physical. [Character locomotion state machine](UML/character-locomotion-state-machine.drawio.svg)

## Forms and stability

- Left Shift requests a form change. Enter Shadow only while the foot anchor is lit and the destination form fits at the current position. Return to Physical whenever the destination form fits, including during the outside-light grace period.
- Physical collides with Physical Solids and Universal Solids and passes Shadow Barriers. Shadow passes Physical Solids and collides with Shadow Barriers and Universal Solids. Both forms collide with the ground and platforms. Reject a change that would overlap a blocker for the destination form.
- Leaving light as Shadow starts the configurable one-second stability countdown and suppresses movement input. Light can still be dragged or aimed back onto the character. Returning to light resets the countdown. A successful change to Physical clears it. Expiry triggers failure and restart.

## Integration order

1. While `Playing`, update left-button lamp dragging, then right-button aiming, and capture the resulting footprint. Read A/D, W, and Left Shift independently from the mouse buttons.
2. Evaluate a Left Shift request against that footprint, the current foot sample, and destination collision clearance. Apply a successful form change before the next movement step. Update shadow stability using the resulting form. [Character form state machine](UML/character-form-state-machine.drawio.svg)
3. Pass horizontal input and a valid grounded jump request to locomotion when movement is permitted. Consume rejected jump requests so returning to light or landing requires a new W press.
4. Let the level session handle hazards, falling out of bounds, completion, pause, and restart. Gate movement, form, and lamp input outside `Playing`. Read R and Escape as session inputs. On reset, restore character position, velocity, Physical form, lamp position and aim, stability timer, and movement state. Rebuild the light footprint before evaluating the reset character. Clear pending jump requests and mouse drag baselines. Require held mouse buttons to be released after reset or resume before a new gesture. [Level session state machine](UML/level-session-state-machine.drawio.svg)

## Greybox acceptance

| Scenario | Expected result |
| --- | --- |
| A · Move with A/D, press W while grounded, and press W again before landing. | The character moves directly and can steer in the air. The second press does not create another jump. Holding W through landing does not trigger a new jump. |
| A · Move or jump while holding the left mouse button, then the right button, then both. | Dragging moves the lamp along its rail, aiming rotates the cone, and character input continues to work. Releasing buttons preserves lamp position and aim. |
| B · Walk into a Physical Solid, illuminate the character, and press Left Shift. Try switching back inside the wall. | Physical is blocked, Shadow can cross, and the change back is refused until the destination form fits. |
| B · Reach the Shadow Barrier in Shadow, then return to Physical while clear of blockers. | Shadow is blocked and Physical can pass. Universal Solids block both forms. |
| C · Cross the long Physical Solid as Shadow while dragging and aiming the lamp. | The player can keep the foot anchor inside the visible cone throughout the crossing and reach the exit. |
| C · Move or aim the light away from Shadow, then restore illumination before expiry. | Horizontal steering and jumping stop while gravity continues. Restoring light resets the countdown and permits current movement input. A new W press is required to jump. |
| C · Leave Shadow outside light until the timer expires, then restart with R during a later run. | Both resets restore the character, form, lamp position and aim, timer, velocity, and movement state. |
| Press Escape while dragging, aiming, moving, or jumping. Resume and release the mouse buttons. | Gameplay freezes during pause. The light retains its position and aim, and a new mouse gesture starts after release. |

## First playable handoff

- The gameplay owner can implement the cone and visible boundary against the lamp's position and rotation while wiring direct movement and form collision.
- The main scene owner can place greybox geometry and position markers in parallel. Integrate the A–C checks in the **13 November First Playable** build.
