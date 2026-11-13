# Game Off 2026 art and demo issue drafts

Reviewed on 12 November 2026 against `YYchainsAw/unity-shift-jam` at `origin/main` commit `6fa774a`.

Eight programming and release issues and four milestones were published on 12 November 2026. PR #2 has merged the direct controls, state machines, form collisions, spotlight dragging and aiming, stability, Physical switch, Shadow updraft, and playable upper exit. The art entries below remain task proposals for coordination with the shared art checklist.

The art canvas contains 22 T0 rows, 16 T1 rows, and one font row with no priority. Its completion boxes are empty. Existing platform samples and procedural visuals provide integration references, while production asset delivery remains to be verified.

Source references are the art canvas `canvas_GameOff2026-261112_0303.pdf`, the five-page `Shadow Shift - Demo Level Guide.pdf`, the technical GDD, and the story pitch. Current controls follow the confirmed direct-control design.

## Jam deadline and delivery scope

The [official Game Off 2026 page](https://itch.io/jam/game-off-2026) gives the submission deadline as 1 December 2026 at 13:37 PST, or 2 December 2026 at 05:37 UTC+08:00. It also requires a public GitHub source repository. The repository is currently private, so the release task includes confirming public access with the repository owner.

The proposed jam baseline is one complete level built from the merged ShadowShift route, with readable Physical/Shadow visuals, the light-controlled fan puzzle, contextual controls, failure/restart/completion feedback, and a verified Windows build. The five-world story and the full five-phase Demo Guide provide additional backlog scope.

All dates below use UTC+08:00. Internal milestones are planning targets, with 29 November at 21:00 retained from the existing development plan to leave submission buffer. GitHub stores milestone due dates at date granularity, so the exact Beijing times are recorded in their descriptions.

| Target | Outcome |
| --- | --- |
| 18 November | First production art pass integrated with the current playable route and a new-player test |
| 23 November | Content lock for the jam level, with chosen art, mechanics, and layout integrated |
| 27 November | Release candidate built from a clean checkout and tested on another device |
| 29 November at 21:00 | Internal target for the itch.io submission and source-access verification |
| 2 December at 05:37 | Official submission deadline |

## Planning batches

| Batch | Drafts | Suggested milestone |
| --- | --- | --- |
| Jam art and playable integration | A01–A07, G01 | First Art Integration, 18 November |
| Build, playtest, and submission | R01 | Release Candidate checkpoint on 27 November, then Internal Submission on 29 November |
| Presentation improvements | A08–A12 | Select for the content lock according to available capacity |
| Extended demo loop | G02–G07 | Stretch backlog, selected according to capacity before content lock |

The published milestones are [First Art Integration](https://github.com/YYchainsAw/unity-shift-jam/milestone/1), [Content Lock](https://github.com/YYchainsAw/unity-shift-jam/milestone/2), [Release Candidate](https://github.com/YYchainsAw/unity-shift-jam/milestone/3), and [Internal Submission](https://github.com/YYchainsAw/unity-shift-jam/milestone/4). Their target dates are 18, 23, 27, and 29 November. The first three internal targets use 23:59 Beijing time, and submission uses 21:00.

The repository accounts available for assignment are `Uniwhyse`, `vitroell`, and `YYchainsAw`. The published issues are open for team ownership.

## Published programming issues

| Planning reference | GitHub issue | Milestone | Scope |
| --- | --- | --- | --- |
| G01 | [#3 - Integrate production art and player-facing feedback](https://github.com/YYchainsAw/unity-shift-jam/issues/3) | First Art Integration | Jam baseline |
| G02 | [#4 - Add form-dependent descent and Shadow slow fall](https://github.com/YYchainsAw/unity-shift-jam/issues/4) | Content Lock | Stretch |
| G03 | [#5 - Add a Shadow-operated switch for the upper route](https://github.com/YYchainsAw/unity-shift-jam/issues/5) | Content Lock | Stretch |
| G04 | [#6 - Add spotlight occlusion and matching visible illumination](https://github.com/YYchainsAw/unity-shift-jam/issues/6) | Content Lock | Stretch |
| G05 | [#7 - Break cracked floors with a Physical landing impact](https://github.com/YYchainsAw/unity-shift-jam/issues/7) | Content Lock | Stretch |
| G06 | [#8 - Charge a battery and hand control to the next lamp](https://github.com/YYchainsAw/unity-shift-jam/issues/8) | Content Lock | Stretch |
| G07 | [#9 - Assemble and playtest the complete demo loop](https://github.com/YYchainsAw/unity-shift-jam/issues/9) | Release Candidate | Stretch |
| R01 | [#10 - Build, playtest, and submit the Game Off 2026 entry](https://github.com/YYchainsAw/unity-shift-jam/issues/10) | Internal Submission | Jam baseline |

Gameplay implementation can use the current placeholder art while production art progresses in parallel. G01 and G07 bring the delivered assets into the integrated level.

Readable gameplay prompts are part of G01. A11 supplies the presentation artwork when available. The canvas's T0/T1 values remain recorded on their art tasks, while the proposed batches above determine the jam delivery order. The five-number level-selection kit is a presentation task whose navigation scope follows the confirmed release level count.

## A01 · Confirm the art handoff and demo asset contracts

Labels `type:docs`, `area:art`. Proposed prerequisite for the first art batch. The font row retains its unspecified source priority.

### Task

Record one agreed export and integration reference for the artists and programmer, using the existing ShadowShift scene and the new art canvas.

### Acceptance criteria

- [ ] Record the reference Game view, sprite canvas dimensions, tile dimensions, PPU, pivots, naming, file formats, animation frame order, and playback rates.
- [ ] Review the scale sample in `01_Level_Start`. The 32 px platform tiles use 32 PPU. The 64 px character frames use 64 PPU with `Visual` scaled to `(2, 2, 1)`. Record the agreed production export settings.
- [ ] Resolve the canvas's Tilemap handoff against the current SpriteRenderer-authored scene. Define the tileset delivery and any Tile Palette integration work.
- [ ] Map the existing Idle, Moving, Jumping, Falling, and Rising states to the six requested Physical/Shadow animations, including the pose to reuse for each upward state.
- [ ] Record the English font and its redistribution license, and confirm the displayed game title for the logo and cover.
- [ ] Record any extra art needed for selected stretch mechanics, including the Shadow switch, battery, and cracked-floor broken state.
- [ ] If G03 is selected for the jam, record its latch or light-dependent policy. The Demo Guide marks loss-of-light deactivation as optional.
- [ ] Align the form-switching wording with the current behavior, which requires light to enter Shadow and allows returning to Physical when the destination fits.

### Additional context

Use `Assets/Scenes/ShadowShift.unity`, `Assets/Art/Environment/Tilesets/Platform.png`, and `Docs/Development/first-playable.md` as the current integration references. Art exploration and roughs can proceed while the export values are recorded.

## A02 · Create Physical character animations

Labels `type:feature`, `area:art`. Source priority T0. Depends on the handoff values in A01.

### Task

Deliver the Physical form's Idle, Running, and Falling sprite animations.

### Acceptance criteria

- [ ] Deliver all three animation sets with the agreed canvas size, frame order, and playback rates.
- [ ] Keep the character's foot position and apparent scale consistent across frames and states.
- [ ] Record facing direction and the intended horizontal-flip behavior.
- [ ] Preview the sprites against the current room background and spotlight.
- [ ] Include editable sources and agreed exports, with Unity metadata for imported assets.

### Additional context

The art canvas lists three Physical animations. The state-to-animation mapping from A01 covers Jumping and Rising. Integration is tracked in G01.

## A03 · Create Shadow character animations

Labels `type:feature`, `area:art`. Source priority T0. Depends on A01.

### Task

Deliver the Shadow form's Idle, Running, and Falling sprite animations.

### Acceptance criteria

- [ ] Deliver all three animation sets using the Physical form's agreed scale and foot anchor.
- [ ] Make the form identifiable against the room background and inside the beam.
- [ ] Record frame order, playback rates, facing direction, and horizontal-flip behavior.
- [ ] Preview the animation and form changes in the same in-engine scale sample as A02.
- [ ] Include editable sources, agreed exports, and Unity metadata for imported assets.

### Additional context

The art canvas lists three Shadow animations. The existing character uses a shared collider and foot anchor across both forms.

## A04 · Create spotlight rail and fixture artwork

Labels `type:feature`, `area:art`. Source priority T0. Depends on A01.

### Task

Deliver a horizontally tileable rail and a spotlight fixture that fits the existing movable lamp pivot. Review the current procedural cone's appearance against the art direction.

### Acceptance criteria

- [ ] The rail tiles horizontally with aligned edges at the agreed scale.
- [ ] The fixture's pivot matches the beam origin while moving and aiming.
- [ ] Provide the agreed beam color and edge treatment using an in-engine preview.
- [ ] Keep the visible cone and gameplay illumination boundary aligned during aiming.
- [ ] Include editable sources, exports, and Unity metadata for imported assets.

### Additional context

This covers the canvas's rail, fixture, and beam rows. `SpotlightRail` already renders the beam programmatically, matching the canvas's suggested simple treatment.

## A05 · Create a reusable level background

Labels `type:feature`, `area:art`. Source priority T0. Depends on A01.

### Task

Deliver a side-view background suitable for the teaching route and the main demo room.

### Acceptance criteria

- [ ] Deliver a background that covers the agreed camera view and the vertical fan room.
- [ ] Document horizontal extension or tiling behavior where the camera moves.
- [ ] Keep the player, form-specific walls, switch, wind, and beam readable over the background.
- [ ] Review an in-engine screenshot at the target Game view and export scale.
- [ ] Include editable sources, exports, and Unity metadata for imported assets.

### Additional context

The canvas explicitly permits the tutorial and main level to share a background. The story pitch's first-world abandoned-room direction can guide the visual review.

## A06 · Create matching dark and illuminated environment tiles

Labels `type:feature`, `area:art`. Source priority T0. Depends on A01.

### Task

Deliver matched dark and illuminated tiles for walkable ground, walls passable in Shadow, walls passable in Physical, and breakable floor.

### Acceptance criteria

- [ ] Deliver all four tile categories in both appearances, covering the eight canvas rows.
- [ ] Match dimensions, pivots, and edge geometry between each dark/lit pair.
- [ ] Preview adjoining tiles, exposed ends, and floor/wall joins for visible seams.
- [ ] Make both form-specific wall types identifiable using the agreed visual language.
- [ ] Supply the cracked and broken-floor appearances agreed in A01.
- [ ] Demonstrate the agreed SpriteRenderer or Tilemap import workflow in the existing scene.

### Additional context

Use `Platform.png` as the current scale and slicing sample. The rule implementation for cracked floors is tracked in G05. Illumination rendering and occlusion are tracked in G04.

## A07 · Create the pressure plate, fan, and updraft artwork

Labels `type:feature`, `area:art`. Source priority T0. Depends on A01.

### Task

Replace the current mechanism placeholders with the Physical pressure plate, fan, and active-airflow artwork requested by the canvas.

### Acceptance criteria

- [ ] Show distinguishable idle and pressed plate states.
- [ ] Show distinguishable unpowered and powered fan states.
- [ ] Communicate the active updraft's location and upward direction while keeping the character visible.
- [ ] Fit the agreed collider and visual anchors in the existing fan room.
- [ ] Validate the plate indicator and active airflow with `PhysicalSwitch` and `ShadowUpdraft`.

### Additional context

PR #2 implements the green switch and illuminated Shadow lift. This task delivers their production appearance. Additional Shadow-switch artwork is recorded through A01 and G03.

## A08 · Create title screen and button artwork

Labels `type:feature`, `area:art`. Source priority T1. Depends on the title and export decisions in A01.

### Task

Deliver the title background, game logo, and Start, Settings, and Quit button artwork.

### Acceptance criteria

- [ ] Deliver the background and agreed game logo.
- [ ] Deliver Normal, Hover, and Pressed states for all three buttons.
- [ ] Preview the complete layout at the agreed reference resolution.
- [ ] Identify text, editable labels, and image regions for Unity UI assembly.

### Additional context

This covers the four title-screen rows in the canvas. Runtime menu behavior can be scoped after the presentation layout review.

## A09 · Create settings screen controls

Labels `type:feature`, `area:art`. Source priority T1. Depends on A01 and the shared button style from A08.

### Task

Deliver the settings layout and control artwork for display mode, SFX volume, and master volume.

### Acceptance criteria

- [ ] Include fullscreen/windowed selection, SFX volume, and master-volume controls.
- [ ] Supply slider backgrounds, fills, handles, and relevant interaction states.
- [ ] Identify each control's editable label and artwork in the handoff.
- [ ] Preview the layout at the agreed reference resolution.

### Additional context

This covers the three settings rows. Audio assets and actual mixer wiring need their own implementation scope when the controls are integrated.

## A10 · Create the level-selection artwork kit

Labels `type:feature`, `area:art`. Source priority T1. Depends on A01 and confirmation of the intended level-selection flow.

### Task

Deliver the level tile, numbers 1–5, selection/click feedback, selection background, and locked state.

### Acceptance criteria

- [ ] Cover all five level-selection rows in the canvas.
- [ ] Keep normal, selected, and locked tile appearances distinguishable.
- [ ] Supply number artwork or the agreed editable-font treatment.
- [ ] Preview the layout at the agreed reference resolution.
- [ ] Record which levels are available in the intended release so the UI can be wired accurately.

### Additional context

The current build enables one scene. The canvas's five number assets and the story's five worlds need a release-scope decision before navigation behavior is implemented.

## A11 · Create instruction and control prompt artwork

Labels `type:feature`, `area:art`. Source priority T1. Depends on A01.

### Task

Deliver the instructional dialog box and reusable control prompts.

### Acceptance criteria

- [ ] Provide a readable dialog/prompt container and its text-safe area.
- [ ] Cover A/D movement, W jump, Left Shift form change, left-button rail drag, right-button aiming, R restart, and Escape pause/resume.
- [ ] Identify editable text and key/mouse icon elements for Unity UI assembly.
- [ ] Preview prompts over the room background and spotlight.

### Additional context

The canvas lists A/D and mouse prompts. The additional controls above follow the confirmed design and existing implementation. Player-facing integration is tracked in G01.

## A12 · Create the game icon and cover

Labels `type:feature`, `area:art`. Source priority T1. Depends on the title and visual style agreed in A01.

### Task

Deliver the promotional game icon and cover artwork.

### Acceptance criteria

- [ ] Deliver a 1:1 game icon and 16:9 game cover.
- [ ] Record exported pixel dimensions for the intended upload targets.
- [ ] Verify readability at the actual presentation size.
- [ ] Include editable source files and export any title treatment separately when required by the agreed layout.

### Additional context

This covers the canvas's two promotional rows. Final upload dimensions can be checked against the chosen distribution target during release preparation.

## G01 · Integrate production art and player-facing feedback

Labels `type:feature`. Proposed demo integration task. Integrate relevant A01–A07 deliveries as they arrive. A11 supplies final prompt styling, while readable prompts can be developed with the current UI.

### Task

Integrate the first production art pass into ShadowShift and connect animation and contextual feedback to the existing state machines.

### Acceptance criteria

- [ ] Map locomotion states and form changes to the animation mapping agreed in A01.
- [ ] Use the agreed target of approximately 1.75 terrain cells in visible character height. Confirm the original frame canvas with the artist and align both forms to a shared foot anchor.
- [ ] Fit the character collider to the agreed body dimensions and verify doors, ceilings, upper landings, wind coverage, and camera framing against the new scale.
- [ ] Connect switch, fan, light, and form visuals to their gameplay states.
- [ ] Teach movement, jumping, shifting, dragging, and aiming at the point where each action is needed.
- [ ] Show invalid-shift feedback, light-loss warning, pause, failure, and completion in the player-facing presentation.
- [ ] Verify the current fan route from spawn to exit and R reset in a Windows build.

### Additional context

The current `LevelSession` provides a debug-style HUD and tints the shared character visual. Current art and mechanism placeholders already provide integration anchors.

## G02 · Add form-dependent descent and Shadow slow fall

Labels `type:feature`. Stretch demo mechanic. Record movement tuning with A01 and use the current character visuals during implementation.

### Task

Implement the Shadow glide needed to approach the lower purple switch and preserve the cracked floor in Demo Guide phases 1 and 2.

### Acceptance criteria

- [ ] From the same drop height, Shadow descends more slowly than Physical using configurable values.
- [ ] Midair form changes immediately apply the agreed descent behavior.
- [ ] Horizontal steering and light-loss stability work during the descent.
- [ ] Updraft Rising, pause/resume, landing, and restart restore the appropriate motion and gravity.
- [ ] Demonstrate a reachable lower-switch approach and an intact-floor landing in the authored demo route.

### Additional context

`CharacterFallState` currently applies one fall multiplier to both forms. Extend the existing locomotion/form integration.

## G03 · Add a Shadow-operated switch for the upper route

Labels `type:feature`. Stretch demo mechanic. Use the switch policy recorded in A01 and the current pressure-plate visual during implementation. G01 integrates the production appearance.

### Task

Add the lower purple switch and its upper-route response from the Demo Guide.

### Acceptance criteria

- [ ] Shadow activates the switch and Physical leaves it inactive.
- [ ] Activation enables the authored upper route with visible feedback.
- [ ] Implement the agreed latch or light-dependent lifetime from A01.
- [ ] Pause and restart preserve or reset state according to the level lifecycle.
- [ ] The existing green Physical switch continues to power the fan correctly.

### Additional context

`PhysicalSwitch` currently supports the green switch. The optional loss-of-light deactivation in the Demo Guide needs the recorded policy before implementation.

## G04 · Add spotlight occlusion and matching visible illumination

Labels `type:feature`. Stretch demo prerequisite. Record the demo's occluder types and implement against the current scene geometry. G01 integrates the A06 visual treatment.

### Task

Make purple barriers and intact cracked floors block the demo light path, using one visibility result for the player, switches that require illumination, and battery charging.

### Acceptance criteria

- [ ] An authored purple barrier or intact cracked floor blocks light reaching a target behind it.
- [ ] Opening or breaking the relevant surface restores visible illumination through the opening.
- [ ] The rendered light boundary and gameplay visibility agree while dragging and rotating the lamp.
- [ ] Existing lit form entry, shadow stability, and updraft eligibility use the same visibility result.
- [ ] Dark/lit environment presentation follows the delivered visual treatment.
- [ ] Define and test beam-edge behavior and multiple overlapping blockers.

### Additional context

`SpotlightRail.Contains` currently checks the finite cone's geometry. The Demo Guide adds purple-wall occlusion and the cracked-floor opening used to reach the battery.

## G05 · Break cracked floors with a Physical landing impact

Labels `type:feature`. Stretch demo mechanic. Develop impact detection with the current floor visuals, then verify its combined behavior with G02, G04, and the A06 floor states during integration.

### Task

Let the upper-loop return drop break a cracked floor when the character lands as Physical with sufficient impact.

### Acceptance criteria

- [ ] A configurable Physical impact threshold breaks the authored cracked floor.
- [ ] The Shadow slow-fall approach preserves that same floor.
- [ ] Art, collision, and light occlusion update together when the floor breaks.
- [ ] Restart restores the initial floor appearance, collider, and occlusion.
- [ ] The authored upper route provides the required drop and a safe continuation to the lower passage.

### Additional context

This implements phase 4 of the Demo Guide and prepares the battery's light path. The existing player and form state machines remain the motion entry points.

## G06 · Charge a battery and hand control to the next lamp

Labels `type:feature`. Stretch demo mechanic. Develop charging and lamp control with placeholder battery art alongside G04 and G05. G07 validates the complete light path and production visuals.

### Task

Add the battery under the cracked floor and the next-room lamp handoff from phase 5 of the Demo Guide.

### Acceptance criteria

- [ ] The battery charges while illuminated through a valid unblocked light path.
- [ ] Charging amount and charge-complete state have visible feedback.
- [ ] A full battery powers the next authored lamp.
- [ ] The player can enter the next room and control its lamp with the confirmed mouse controls.
- [ ] Pause and restart handle battery charge, selected lamp, lamp poses, and power consistently.
- [ ] Demonstrate the complete cracked-floor opening and battery handoff in the level.

### Additional context

The current `LevelSession` owns one spotlight. Extend that integration for the authored next-room transition and record the intended charge-retention behavior before tuning the puzzle.

## G07 · Assemble and playtest the complete demo loop

Labels `type:feature`. Stretch demo acceptance task. Depends on G01–G06 and the required art deliveries. R01 verifies the selected jam baseline independently of this larger loop.

### Task

Assemble the Demo Guide's five phases in the current authored scene and verify the full player journey in a distributable build.

### Acceptance criteria

- [ ] The route connects the Physical switch, Shadow switch, fan ascent, upper form-specific platform, Physical impact, battery charge, and next-lamp handoff.
- [ ] Each required form, light placement, and light rotation has a readable cue and a recoverable test case.
- [ ] Verify pause, hazards, light-loss failure, restart, completion, and the authored recovery route after a missed jump.
- [ ] Build and launch the chosen Windows baseline, then complete the full route using normal keyboard and mouse input.
- [ ] Two players unfamiliar with the route try the build. Record completion time and the places where they need help.
- [ ] Update the playable guide and relevant diagrams to match the verified layout.

### Additional context

The current vertical fan level is the working baseline. The Demo Guide provides the larger loop, while `Docs/DevelopmentPlan.md` records the current 5–8 minute first-playthrough target.

## R01 · Build, playtest, and submit the Game Off 2026 entry

Labels `type:feature`. Proposed jam release task. Target the release candidate on 27 November and submission on 29 November at 21:00 UTC+08:00. The official deadline is 2 December at 05:37 UTC+08:00.

### Task

Deliver the selected, content-locked ShadowShift level as a playable Game Off 2026 entry with accessible source code and complete release information.

### Acceptance criteria

- [ ] Build the Windows player from a clean checkout in Unity 6000.3.10f1 with Git LFS assets present.
- [ ] Complete the selected level with normal keyboard and mouse input on another device, covering both forms, the light puzzle, fan ascent, exit, pause, failure, and restart.
- [ ] Record two unfamiliar players' completion time and points of confusion, then verify blocking fixes in the release candidate.
- [ ] Record the source commit, build version, controls, credits, asset and font attributions, and tested platform.
- [ ] Upload the playable build to its itch.io project page and enter it into Game Off 2026 before the official deadline.
- [ ] Confirm the repository owner's authorization for public source access, then verify the submitted source link and LFS assets as a visitor.
- [ ] Verify the game page, downloadable build, and jam submission link as a visitor.

### Additional context

The proposed release scope is the single complete level defined above. The [official jam page](https://itch.io/jam/game-off-2026) provides the submission period and source-publication requirement. The existing repository development plan sets 29 November at 21:00 UTC+08:00 as the internal submission target.
