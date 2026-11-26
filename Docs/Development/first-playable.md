# First playable level

> [!NOTE]
> The sections from **Run the test** through **Acceptance checks** describe the legacy `ShadowShift` greybox scene, kept disabled in the build list for reference. The current release progression comprises three studio stages (`StudioLevel1`, `StudioLevel2`, and `StudioDemo`). Start from `MainMenu` (build index 0) and follow the [Studio demo route](#studio-demo-route).

The current layout starts in the upper left and leads through the blue wall on the middle landing to the green fan switch and upper-right landing. The lower platforms hold the purple wall, purple switch, and cracked floor. The battery marker is below the cracked floor. [Level layout](../Design/first-playable-greybox.drawio.svg)

## Run the test

1. Open [ShadowShift.unity](../../Assets/Scenes/ShadowShift.unity) in Unity 6000.3.10f1.
2. Set the Game view to **16:9**, enter Play mode, and focus the Game view.
3. Use the controls below.

The build scene list enables `MainMenu`, `StudioLevel1`, `StudioLevel2`, and `StudioDemo`, with `ShadowShift` disabled.

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
| Descent to the lower platforms | Shadow uses lower fall gravity and a fall speed limit. | Use the slower Shadow descent to steer toward the lower switch. |
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
| Step off the outside edge of the bottom floor. | The level restarts after a short delay. |
| Step onto the green switch as Shadow, then as Physical. Enter the active wind column in Physical form. | The switch responds to Physical and stays on after the character steps off. Physical receives no lift. |
| Enter the powered wind as lit Shadow. | The wind lifts Shadow while the character's feet remain in the beam. |
| Reach the blue section of the upper platform after the ascent. | Physical can land on the platform. Shadow passes through it. |
| Reach the finish trigger on the bottom floor and press `R`. | The trigger completes the level. Restart returns the character to spawn, resets the lamp, and turns the fan off. A failure also resets the fan. |

Record whether players can move, jump, and aim comfortably at the same time, and whether they can predict when the character will leave the cone.

## Studio demo route

Start from [MainMenu.unity](../../Assets/Scenes/MainMenu.unity) (**Play** → **1**, **2**, or **3**) or open [StudioLevel1.unity](../../Assets/Scenes/StudioLevel1.unity), [StudioLevel2.unity](../../Assets/Scenes/StudioLevel2.unity), and [StudioDemo.unity](../../Assets/Scenes/StudioDemo.unity) directly.

During descent, Shadow uses 0.7× gravity and a 4.5-unit/s fall speed limit by default. Physical uses 1.4× gravity to build impact speed. A successful midair shift applies the new descent settings immediately. Jumping includes coyote time (0.10s) and landing jump buffering (0.12s).

Instead of a top-left text box, guidance is diegetic:
- **Overhead badge** above the character shows `Shift` when lit in Physical form, `Need Light` or `Blocked` briefly when a shift is denied, and the remaining stability countdown (`0.8s`) if Shadow leaves the beam.
- **Circuit wires** connect switches and batteries directly to the mechanisms they power, lighting up and sending pulses once activated.
- **Stage checkpoints** preserve already-pressed switches on automatic 0.5s retry (including pre-break lower-floor falls in Stage 3). Press `R` for a full stage reset.

### Stage 1 (`StudioLevel1`) — Light & Form
1. Walk right (`A`/`D`) and jump (`W`) up the starter step.
2. Aim the spotlight at your feet (`Right Mouse`) and press `Left Shift` to pass the first blue gate as Shadow.
3. Hold `Left Mouse` to drag the spotlight right along its rail while staying in Shadow through the three closely spaced blue gates.
4. Once clear of the third blue gate, press `Left Shift` to return to Physical, walk through the violet barrier, and enter the lit exit. Choose **Next level** to continue.

### Stage 2 (`StudioLevel2`) — Wind & Conduit
1. Walk right on the starting platform as Physical to press the **green switch**, lighting the green wire to the fan.
2. Jump right to the middle platform, aim the spotlight at your feet, shift to Shadow through the **blue wall**, and step onto the **violet switch** to light the violet wire and solidify the upper walkway.
3. Slide the spotlight over the fan column, hop right into the lit updraft as Shadow, rise past the upper walkway, and move right across the walkway into the exit. Choose **Next level** to continue.

### Stage 3 (`StudioDemo`) — Shatter & Charge
1. On the upper-left platform, aim lamp 1 at your feet, shift to Shadow through the blue wall, and step on the **violet switch** to enable the upper walkway.
2. Walk off the right edge onto the cracked vault deck, shift to Physical to pass the **purple wall**, and step onto the **green switch** to start the fan right ahead.
3. Slide lamp 1 over the updraft column, hop right into the wind, shift to Shadow, rise clear of the upper walkway, and move left onto it.
4. Walk to the left end of the upper walkway directly over the cracked floor, shift to Physical, and drop straight down to **smash the cracked floor**.
5. Slide lamp 1 over the broken opening and aim down at the **battery** for **1.25 seconds** total (partial charge is retained). The cyan wire powers lamp 2 and unlocks the exit.
6. Walk right along the bottom floor to the pulsing **Lamp 2** pad to hand mouse control to lamp 2. Release held mouse buttons before starting a new gesture.
7. Drag lamp 2 left to light your feet at the final blue gate, shift to Shadow, and enter the exit.

### Sound effects

The Studio Demo uses the following Kenney clips under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). The project copies retain the original audio with filenames based on their in-game cues.

| Cue | Original file | Source |
| --- | --- | --- |
| Shift | `maximize_003.ogg` | [Interface Sounds](https://kenney.nl/assets/interface-sounds) |
| Cracked floor breaks | `impactMining_002.ogg` | [Impact Sounds](https://kenney.nl/assets/impact-sounds) |
| Battery fully charged | `confirmation_004.ogg` | [Interface Sounds](https://kenney.nl/assets/interface-sounds) |
| Control switches to lamp 2 | `toggle_001.ogg` | [Interface Sounds](https://kenney.nl/assets/interface-sounds) |
| Room complete | `Pizzicato jingles/jingles_PIZZI02.ogg` | [Music Jingles](https://kenney.nl/assets/music-jingles) |
