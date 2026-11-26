# shadowshift development handoff

Snapshot updated on 26 November 2026. The project is a Unity 2D puzzle platformer set in an abandoned film studio, built for Game Off 2026. The player moves and aims a spotlight while switching between Physical and Shadow across three progressive studio stages (`StudioLevel1`, `StudioLevel2`, and `StudioDemo`).

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
2. Open [MainMenu.unity](../../Assets/Scenes/MainMenu.unity), set Game view to **16:9**, and enter Play Mode. Choose **Play** → **1**, **2**, or **3**. Open [StudioLevel1.unity](../../Assets/Scenes/StudioLevel1.unity), [StudioLevel2.unity](../../Assets/Scenes/StudioLevel2.unity), or [StudioDemo.unity](../../Assets/Scenes/StudioDemo.unity) directly when iterating on gameplay.
3. Follow the [three-stage walkthrough](first-playable.md#studio-demo-route) for a full manual playthrough. The earlier sections of that document cover the legacy `ShadowShift` greybox.

The [build scene list](../../ProjectSettings/EditorBuildSettings.asset) enables `MainMenu` (0), `StudioLevel1` (1), `StudioLevel2` (2), and `StudioDemo` (3). `ShadowShift` remains listed (disabled) as a legacy greybox. `01_Level_Start` is a separate scale and art sample.

## Player-facing behavior

- **Main menu**. Play turns the hanging lamp left, then reveals a single row of level numbers `1  2  3`, all unlocked by default for jam evaluation. Back reverses the sweep and restores the home menu.
- **Controls, Credits, Quit**. Controls shows the real input bindings. Credits lists Game Off 2026 and Kenney audio. Quit opens a confirmation with Cancel initially selected. The editor quits Play Mode on confirmation. Standalone uses `Application.Quit()`. The WebGL code displays a close-tab instruction.
- **Menu focus**. Play starts selected. Mouse movement lights only the hovered button. Pointer exit clears its highlight while retaining the selection needed for keyboard navigation. Arrow keys and Enter navigate and submit. Esc returns from menu subpages.
- **In-game menus**. Esc pauses and resumes. The pause panel has Resume, Restart, and Main Menu. Stages 1 and 2 offer **Next level**, **Play again**, and **Main Menu** on completion; Stage 3 offers **Play again** and **Main Menu**. Entering a stage from the title screen starts a fresh session.

| Gameplay input | Action |
| --- | --- |
| `A` / `D` | Move left or right |
| `W` | Jump |
| `Left Shift` | Request a form change |
| Hold and drag left mouse | Slide the controlled lamp along its rail |
| Hold right mouse | Aim the beam at the cursor |
| `Esc` | Pause or resume |
| `R` | Reset the entire room |

## Three-stage progression and rules

- **Stage 1 (`StudioLevel1`) — Light & Form**: Compact fixed-frame onboarding (`x ∈ [0, 22]`) with diegetic keycap callouts. Walk right, jump the starter step, aim the spotlight with right mouse and shift to Shadow through the blue colonade while dragging the lamp with left mouse, then shift back to Physical to pass the violet barrier and enter the open exit.
- **Stage 2 (`StudioLevel2`) — Wind & Conduit**: Two-tier fixed-frame room (`x ∈ [0, 26]`) introducing permanent circuit wires (`CircuitWire`) and ordered checkpoints (`LevelCheckpoint`). Step on the green switch as Physical to power the fan, cross the blue wall as Shadow to press the violet switch and enable the upper walkway, then ride the lit updraft up and right to the exit.
- **Stage 3 (`StudioDemo`) — Shatter & Charge**: Full-hall finale (`x ∈ [-2, 31]`) refactored into a forward-flowing route:
  1. From the upper-left platform, light your feet, shift to Shadow through the blue wall, and step on the violet switch to enable the upper walkway.
  2. Drop right onto the cracked vault deck, shift to Physical through the purple wall, and step on the green fan switch right beside the updraft.
  3. Cover the updraft with lamp 1, shift to Shadow, rise onto the upper walkway, and walk left directly over the cracked floor.
  4. Shift to Physical to smash through the cracked floor, then hold lamp 1 on the exposed battery for **1.25 seconds** of accumulated light to power lamp 2 and the exit via the cyan conduit.
  5. Walk right to the glowing lamp 2 pad, take control of lamp 2, and cross the final blue gate as lit Shadow into the exit.

All three rooms use fixed cameras (`followCharacter = false`) for stable mouse dragging and aiming, and pure in-world guidance (permanent circuit wires + overhead status badge for `Shift`, `Need Light`, `Blocked`, and Shadow stability countdown). Automatic retries respawn at the latest `LevelCheckpoint` (or lamp 2 checkpoint) while preserving solved switches, broken floor, and battery charge; `R` and menu **Restart** perform a full stage reset.

## Where to work

| Area | Primary files and responsibilities |
| --- | --- |
| Menu transitions and scene loading | [MainMenuController.cs](../../Assets/Scripts/UI/MainMenuController.cs) owns CanvasGroup fades, input gating, navigation and scene loading. Scene buttons use Inspector persistent callbacks. |
| Pointer and keyboard interaction | [MainMenuButton.cs](../../Assets/Scripts/UI/MainMenuButton.cs) manages focus and hover without double highlights. Both scenes use `InputSystemUIInputModule` and [InputSystem_Actions.inputactions](../../Assets/InputSystem_Actions.inputactions). |
| Title screen lighting | [MenuSpotlight.cs](../../Assets/Scripts/UI/MenuSpotlight.cs) draws a uGUI mesh and rotates the same fixture around its mount. This effect is separate from gameplay illumination. |
| Gameplay state and retry | [LevelSession.cs](../../Assets/Scripts/Gameplay/Level/LevelSession.cs) coordinates input, mechanisms, illumination, audio, handoff, reset, and the Playing/Paused/Failed/Completed states. |
| Character | [PlayerCharacter.cs](../../Assets/Scripts/Gameplay/Character/PlayerCharacter.cs), [PlayerAnimation.cs](../../Assets/Scripts/Gameplay/Character/PlayerAnimation.cs), and [CharacterFormController.cs](../../Assets/Scripts/Gameplay/Character/CharacterFormController.cs) cover locomotion, animation, shift validation, collision rules, and shadow stability. |
| Gameplay spotlight | [SpotlightInput.cs](../../Assets/Scripts/Gameplay/Lighting/SpotlightInput.cs) manages drag, aiming, and control handoff. [SpotlightRail.cs](../../Assets/Scripts/Gameplay/Lighting/SpotlightRail.cs) clips both rendered and sampled light using the scene's configured occluders. |
| Puzzle mechanisms | [PhysicalSwitch.cs](../../Assets/Scripts/Gameplay/Level/PhysicalSwitch.cs), [ShadowWalkwaySwitch.cs](../../Assets/Scripts/Gameplay/Level/ShadowWalkwaySwitch.cs), [ShadowUpdraft.cs](../../Assets/Scripts/Gameplay/Level/ShadowUpdraft.cs), [BreakableFloor.cs](../../Assets/Scripts/Gameplay/Level/BreakableFloor.cs), [LightBattery.cs](../../Assets/Scripts/Gameplay/Level/LightBattery.cs), and [LevelCheckpoint.cs](../../Assets/Scripts/Gameplay/Level/LevelCheckpoint.cs). |
| In-game presentation | [CircuitWire.cs](../../Assets/Scripts/Gameplay/Level/CircuitWire.cs) renders unpowered/powered conduits with traveling pulses. [LevelGuidance.cs](../../Assets/Scripts/Gameplay/Level/LevelGuidance.cs) manages overhead badges, charge meters, and Stage 1 callouts. [StudioDemoMenu.cs](../../Assets/Scripts/UI/StudioDemoMenu.cs) displays pause, next-level, and completion actions. |

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
