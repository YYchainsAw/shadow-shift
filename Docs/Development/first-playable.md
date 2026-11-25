# First playable level

> [!NOTE]
> The sections from **Run the test** through **Acceptance checks** describe the legacy `ShadowShift` greybox scene, kept at build index 2 for reference. The current playable room is Studio Demo. Start from `MainMenu` (build index 0) or open `StudioDemo` (build index 1), and follow the [Studio demo route](#studio-demo-route).

The current layout starts in the upper left and leads through the blue wall on the middle landing to the green fan switch and upper-right landing. The lower platforms hold the purple wall, purple switch, and cracked floor. The battery marker is below the cracked floor. [Level layout](../Design/first-playable-greybox.drawio.svg)

## Run the test

1. Open [ShadowShift.unity](../../Assets/Scenes/ShadowShift.unity) in Unity 6000.3.10f1.
2. Set the Game view to **16:9**, enter Play mode, and focus the Game view.
3. Use the controls below.

The build scene list starts with `MainMenu` and `StudioDemo`. `ShadowShift` is at index 2.

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

Open [StudioDemo.unity](../../Assets/Scenes/StudioDemo.unity) to test the art pass with a fixed view of the whole room.

During descent, Shadow uses 0.7× gravity and a 4.5-unit/s fall speed limit by default. Physical uses 1.4× gravity to build impact speed. A successful midair shift applies the new descent settings immediately. Tune both forms in the Player's **Jump and Fall** settings.

The objective panel briefly explains why a shift was denied. If Shadow leaves the beam, the panel shows the remaining stability time, which freezes on pause. Relighting the character's feet restores steering. Expired stability shows a message and triggers an automatic retry.

If you miss a ledge and reach the bottom floor before opening the crack, the panel prompts you to press `R` and retry. Once you take over lamp 2, an automatic retry returns you to its handoff marker as Physical and resets that lamp's aim and position. The switches, broken floor, and battery charge stay as they were. Press `R` for a full restart at the original spawn.

For a first-time playtest, let the player follow the in-game prompts before sharing the route below. Watch whether they can identify what each switch powers, where to enter the updraft, and why the exit stays dark. The battery meter shows retained charge once the floor breaks.

1. Walk to the right edge of the starting platform, then hold `D` and press `W`. When **Shift** appears above the character, press `Left Shift` and cross the blue wall as Shadow. Wait for **Return to Physical**, switch back, and land on the green fan switch. The **Shift** marker appears when your feet are lit and there is space to enter Shadow.
2. After starting the fan, shift to Shadow and hold `A` to go back through the blue wall. Wait for **Return to Physical** on the left side, then switch back and drop left to the ledge marked **Violet switch**. Stop near the switch, aim at your feet if the **Shift** marker is missing, and step onto the switch as Shadow to enable the upper walkway.
3. Shift to Physical to pass the lower purple wall, then jump right across the gap to the arrow. Slide the first lamp above the diamond and aim down at it. Jump back left from the arrow, shift to Shadow, and release `A` once the wind lifts you. When the objective says **Land on the walkway**, move left and land near its right end. One lamp position covers the entry, ascent, and landing. If you land on the ledge below, jump right as Physical from the middle of that ledge to try again.
4. Walk to the left end of the upper purple walkway. Switch to Physical under the light and steer toward the center of the cracked floor as you fall. The drop from the upper walkway supplies the impact needed to break it. A drop from the fan switch platform leaves it intact.
5. Slide the first lamp over the opening and aim it at the battery. Keep the light on the battery for 1.25 seconds total. Charge pauses in darkness and resumes from the same amount when relit. A full battery powers the second lamp for the rest of the run.
6. Walk right along the lower floor to the **Lamp 2 ready** marker. The first lamp locks in place and the mouse takes control of the second lamp. Release held buttons before starting a new gesture.
7. Drag lamp 2 left to light your feet at the blue wall. Shift to Shadow and jump through the wall toward the stairs. Keep your feet in the second beam as you enter the exit. Press `R` to reset both lamps, their controls, and the room mechanisms.

### Sound effects

The Studio Demo uses the following Kenney clips under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). The project copies retain the original audio with filenames based on their in-game cues.

| Cue | Original file | Source |
| --- | --- | --- |
| Shift | `maximize_003.ogg` | [Interface Sounds](https://kenney.nl/assets/interface-sounds) |
| Cracked floor breaks | `impactMining_002.ogg` | [Impact Sounds](https://kenney.nl/assets/impact-sounds) |
| Battery fully charged | `confirmation_004.ogg` | [Interface Sounds](https://kenney.nl/assets/interface-sounds) |
| Control switches to lamp 2 | `toggle_001.ogg` | [Interface Sounds](https://kenney.nl/assets/interface-sounds) |
| Room complete | `Pizzicato jingles/jingles_PIZZI02.ogg` | [Music Jingles](https://kenney.nl/assets/music-jingles) |
