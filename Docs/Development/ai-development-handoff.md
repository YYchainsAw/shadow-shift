# shadowshift development handoff

Snapshot taken on 25 November 2026. The project is a Unity 2D puzzle platformer set in an abandoned film studio, built for Game Off 2026. The player moves and aims a spotlight while switching between Physical and Shadow. The current playable scope is one complete room, Studio Demo.

## Start here

The code baseline for this handoff is `c3eca10` on `main`. The working branch and GitHub `main` were at that commit before this document was written. Use the repository URL below for a new checkout. It matches the clone example in the root README.

```bash
git lfs install
git clone https://github.com/YYchainsAw/shadow-shift.git
cd shadow-shift
git lfs pull
git status --short --branch
git lfs fsck --objects HEAD
```

1. Open the repository in **Unity 6000.3.10f1**. The project uses URP 2D, uGUI, and Input System only. Check the [Unity version](../../ProjectSettings/ProjectVersion.txt) and [package manifest](../../Packages/manifest.json).
2. Open [MainMenu.unity](../../Assets/Scenes/MainMenu.unity), set Game view to **16:9**, and enter Play Mode. Choose **Play** → **1**. Open [StudioDemo.unity](../../Assets/Scenes/StudioDemo.unity) directly when iterating on gameplay.
3. Follow the [Studio Demo walkthrough](first-playable.md#studio-demo-route) for a full manual playthrough. The earlier sections of that document cover the legacy `ShadowShift` greybox.

The [build scene list](../../ProjectSettings/EditorBuildSettings.asset) enables `MainMenu` at index 0 and `StudioDemo` at index 1. `ShadowShift` remains listed (disabled) as a legacy greybox. `01_Level_Start` is a separate scale and art sample.

## Player-facing behavior

- **Main menu**. Play turns the hanging lamp left, then reveals a single row of level numbers `1  2  3`. Level 1 launches Studio Demo. Levels 2 and 3 are disabled placeholders. Back reverses the sweep and restores the home menu.
- **Controls, Credits, Quit**. Controls shows the real input bindings. Credits lists Game Off 2026 and Kenney audio. Quit opens a confirmation with Cancel initially selected. The editor quits Play Mode on confirmation. Standalone uses `Application.Quit()`. The WebGL code displays a close-tab instruction.
- **Menu focus**. Play starts selected. Mouse movement lights only the hovered button. Pointer exit clears its highlight while retaining the selection needed for keyboard navigation. Arrow keys and Enter navigate and submit. Esc returns from menu subpages.
- **In-game menus**. Esc pauses and resumes. The pause panel has Resume, Restart, and Main Menu. Resume preserves the player's position and suspended velocity. The room-complete panel offers Play again and Main Menu. Entering the room again from the title screen starts a fresh session.

| Gameplay input | Action |
| --- | --- |
| `A` / `D` | Move left or right |
| `W` | Jump |
| `Left Shift` | Request a form change |
| Hold and drag left mouse | Slide the controlled lamp along its rail |
| Hold right mouse | Aim the beam at the cursor |
| `Esc` | Pause or resume |
| `R` | Reset the entire room |

## Studio Demo route and rules

1. Jump into the first lamp's beam. Shift to Shadow to pass the blue wall, then switch back to Physical and land on the green fan switch.
2. Return through the blue wall. Drop to the violet switch ledge, light the player's feet, then press that switch as Shadow to open the upper walkway.
3. Pass the purple wall as Physical, jump to the arrow, and use the first lamp to cover the updraft. Shift to Shadow, ride the wind up, and land on the upper walkway.
4. Drop from the upper walkway as Physical onto the cracked floor. The drop from the fan switch platform is too short to break it.
5. Aim the first lamp at the battery under the floor. It needs **1.25 seconds of accumulated light**. Partial charge persists between exposures.
6. Walk right to the handoff marker and take control of lamp 2. Release held mouse buttons before starting another drag or aim gesture.
7. Stay lit as Shadow while crossing the final blue wall and entering the exit.

The room uses a fixed camera showing the whole level. Physical collides with blue Physical Solids and passes through violet Shadow Barriers. Shadow collides with violet Shadow Barriers and passes through blue Physical Solids. Universal Solids block both. Entering Shadow requires lit feet and a valid destination form. Returning to Physical requires room for that form. Leaving the light suspends Shadow steering and starts a stability timer while gravity continues. After the lamp 2 handoff, automatic retries preserve solved mechanisms and respawn the player as Physical near lamp 2. Restart and `R` reset both lamps, solved mechanisms, and the player to the original spawn.

## Where to work

| Area | Primary files and responsibilities |
| --- | --- |
| Menu transitions and scene loading | [MainMenuController.cs](../../Assets/Scripts/UI/MainMenuController.cs) owns CanvasGroup fades, input gating, navigation and scene loading. Scene buttons use Inspector persistent callbacks. |
| Pointer and keyboard interaction | [MainMenuButton.cs](../../Assets/Scripts/UI/MainMenuButton.cs) manages focus and hover without double highlights. Both scenes use `InputSystemUIInputModule` and [InputSystem_Actions.inputactions](../../Assets/InputSystem_Actions.inputactions). |
| Title screen lighting | [MenuSpotlight.cs](../../Assets/Scripts/UI/MenuSpotlight.cs) draws a uGUI mesh and rotates the same fixture around its mount. This effect is separate from gameplay illumination. |
| Gameplay state and retry | [LevelSession.cs](../../Assets/Scripts/Gameplay/Level/LevelSession.cs) coordinates input, mechanisms, illumination, audio, handoff, reset, and the Playing/Paused/Failed/Completed states. |
| Character | [PlayerCharacter.cs](../../Assets/Scripts/Gameplay/Character/PlayerCharacter.cs), [PlayerAnimation.cs](../../Assets/Scripts/Gameplay/Character/PlayerAnimation.cs), and [CharacterFormController.cs](../../Assets/Scripts/Gameplay/Character/CharacterFormController.cs) cover locomotion, animation, shift validation, collision rules, and shadow stability. |
| Gameplay spotlight | [SpotlightInput.cs](../../Assets/Scripts/Gameplay/Lighting/SpotlightInput.cs) manages drag, aiming, and control handoff. [SpotlightRail.cs](../../Assets/Scripts/Gameplay/Lighting/SpotlightRail.cs) clips both rendered and sampled light using the scene's configured occluders. |
| Puzzle mechanisms | [PhysicalSwitch.cs](../../Assets/Scripts/Gameplay/Level/PhysicalSwitch.cs), [ShadowWalkwaySwitch.cs](../../Assets/Scripts/Gameplay/Level/ShadowWalkwaySwitch.cs), [ShadowUpdraft.cs](../../Assets/Scripts/Gameplay/Level/ShadowUpdraft.cs), [BreakableFloor.cs](../../Assets/Scripts/Gameplay/Level/BreakableFloor.cs), and [LightBattery.cs](../../Assets/Scripts/Gameplay/Level/LightBattery.cs). |
| In-game presentation | [StudioDemoFeedback.cs](../../Assets/Scripts/Gameplay/Level/StudioDemoFeedback.cs) manages hints, charge and route markers. [StudioDemoMenu.cs](../../Assets/Scripts/UI/StudioDemoMenu.cs) displays pause and completion actions using the existing level session. |

The menu uses the approved lowercase title with a solid cream `shadow` and an outlined cyan `shift`. Menu art is under `Assets/Art/Menu/` and studio sprites are under `Assets/Art/Environment/Studio/`. `MenuAtmosphere.png` served the original menu. The current lighting comes from the mesh in `MenuSpotlight`. Player animation uses frame sprites. For audio attribution and source packs, use the [Kenney clip list](first-playable.md#sound-effects).

## Verification and next work

Historical Unity Play Mode checks in a temporary project passed for mouse and keyboard highlight behavior, level selection and loading, Controls/Credits/Quit, pause/resume/restart, returning to the title screen, and entering Studio Demo again. The completion panel and its two actions were exercised by setting the production state machine to Completed. A final compatibility run covered the current menu and a historical scene with the newer page references absent. These were scratch tests. The scripts and temporary scenes were cleaned up. The [PR checks](../../.github/workflows/pr-checks.yml) cover Unity metadata and Git LFS pointers.

**First task for the next developer**. Play the current room from `MainMenu` through the actual exit with ordinary input. Use the in-game hints first and the walkthrough when stuck. Record the first point of confusion or regression and fix that before expanding scope.

- Verify the early jump, Shift prompt, both switches, and the updraft entry and landing.
- Verify occlusion, the Physical-only floor break, interrupted battery charge, the lamp 2 handoff, and the final lit Shadow exit.
- Test pause and resume while airborne or outside the beam. After the handoff, compare an automatic retry with a full restart. Finish through the exit, return to Main Menu, and enter level 1 again.
- Smoke-test menu entry and Back with both mouse and keyboard. Check 16:9 and a narrower Game view for clipping, persistent focus, or click-through.

Current development priority is finishing and polishing the playable room. Level 2 and 3 are outside that scope unless the owner expands it. A full natural-route pass on the handoff commit and a fresh Windows or browser build still need verification. The local Windows archives under `Builds/` are from 11 November and predate the current menus. Package and verify itch.io delivery after feature work, when the owner asks to move into release.

## Known Unity issue and collaboration

- If `MainMenuController.Awake` throws that `controls`, `credits`, or `quit` is unassigned after a script reload, stop Play Mode and reopen the saved `Assets/Scenes/MainMenu.unity`. Check the controller on `Main Menu Canvas` and its groups and buttons in the Inspector. The observed case was an open Unity scene holding empty serialized references while the disk scene already had valid ones. Preserve unsaved edits before reopening. The guard in `c3eca10` lets earlier scenes without the optional pages start and navigate to level 1.
- Keep work on the existing `main` unless the owner requests a branch. The owner prohibits the `codex/` prefix. When a new branch is requested, use the applicable `feature/`, `fix/`, `refactor/`, or `docs/` prefix. Use focused English commit messages. Coordinate commits, pushes, and publication with the owner.
- Keep scene and asset GUIDs stable. Include `.meta` files with newly tracked Unity assets, respect [Git LFS rules](../../.gitattributes), and leave imported sprite sheets unsliced unless slicing is requested. Delete temporary Unity scenes created for verification after use.
- Workstation-only screenshots and art drafts are in `output/art/main-menu/`. The local Git exclude keeps these files outside commits on this machine. On a fresh clone, check ignore rules before staging. The checked-in Unity assets are sufficient to open the project.

## Recent commits

| Commit | Change |
| --- | --- |
| `f72cc8a` | Studio puzzle guidance and retry improvements |
| `f86c31e` | Main menu, title artwork, and button highlighting |
| `95bc6ee` | Spotlight-driven level selection and Studio Demo entry |
| `566c72b` | Controls, Credits, and quit confirmation |
| `7b41627` | Pause, room completion, replay, and return to the title screen |
| `c3eca10` | Guards for absent optional menu page and quit references |
