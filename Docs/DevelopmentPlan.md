# Shadow Shift development plan

## Start here

Build one short 2D puzzle level for Game Off 2026. Target a 5–8 minute first playthrough. The player moves the character with A/D, jumps with W, and switches between Physical and Shadow forms with Left Shift. Hold the left mouse button to drag the spotlight horizontally along its rail, and hold the right mouse button to rotate its beam toward the cursor. The final puzzle asks the player to cross a wall as Shadow while keeping the character lit.

The first playable greybox is due **13 November**. Aim to submit by **29 November at 21:00 UTC+08:00 (13:00 UTC)**. The official deadline is **2 December at 05:37 UTC+08:00 (1 December at 21:37 UTC)**. Submit a playable build through the [Game Off jam page](https://itch.io/jam/game-off-2026) and link the public GitHub source repository. The repository is currently private. The release owner should verify public access by the internal deadline. [Official submission FAQ](https://itch.io/jam/game-off-2026/topic/7290154/frequently-asked-questions-faq)

## First 48 hours

1. **Design lead, 6–7 November.** Share this plan and sketch the level with blocks and arrows. Mark the spawn, rail, light footprint, obstacle types, intended route, shift points, failure or recovery moment, and exit. Write one action the player should discover in each section. Make the sketch and rule decisions available before the art pass.
2. **Repository maintainer, 6–7 November.** Verify team access and ask everyone for their time zone, available hours through 13 November, one task they can own, and any blockers. Ask for replies by **7 November at 12:00 UTC**. Set up the first two GitHub milestones and open the starter issues below as the owners confirm.
3. **Gameplay and integration owners, by 8 November.** Claim the gameplay work and name one owner of the main level scene. Agree on the gameplay rules and the collision and sprite boundaries. In parallel, build and launch the placeholder scene using the intended release target.
4. **Art owners, by 8 November.** Import one sprite per form and one sample obstacle into a Unity test scene. Agree on a reference resolution, pixels per unit, character dimensions, foot pivot, and export format from the actual in-engine view. Use the resulting sample as the reference for later assets.

The design lead owns the rules and scope decisions, maintains the repository, and will provide a starter art sample. Assign gameplay integration and build verification explicitly. One teammate can cover both if capacity allows. If no programmer confirms by 8 November, settle who can implement and integrate the prototype before expanding art production. If only one programmer is available, use a flat route with a few hand-placed obstacles.

## Current controls and greybox rules

The direct character controls and mouse controls below were confirmed on **11 November**. Use this input contract across the greybox, HUD, and acceptance checks.

| Input | Action |
| --- | --- |
| A / D | Move left / right, including in the air. Pressing both produces zero horizontal input. |
| W | Jump on a new press while grounded. Another jump becomes available after landing. |
| Left Shift | Request an immediate change between Physical and Shadow. |
| Hold left mouse button | Drag the lamp horizontally relative to the pointer's drag start, clamped to the rail. |
| Hold right mouse button | Rotate the beam toward the cursor. The beam keeps its aim after release. |
| R | Restart the level. |
| Escape | Pause / resume the level. |

- **Character movement.** A/D supplies horizontal steering and W requests a grounded jump. Read movement and mouse controls independently so the player can move, jump, drag, and aim together. Use gravity and collision for airborne motion and landing.
- **Spotlight.** Hold the left mouse button to translate the lamp along a fixed horizontal rail. Hold the right mouse button to rotate a finite triangular beam around the lamp. The lamp keeps its position and aim when the buttons are released. Both buttons may be held together. Make beam dimensions configurable and use the same cone for the visible boundary and gameplay checks. For this greybox, walls leave the footprint intact. Sample illumination at a shared character foot anchor.
- **Forms.** Physical hits Physical Solids and passes Shadow Barriers. Shadow passes Physical Solids and hits Shadow Barriers. Universal Solids block both. Left Shift switches immediately if the destination form fits. Give feedback when the switch is refused. Apply collision changes before the next movement step.
- **Light stability.** Enter Shadow while lit. If the light moves off the character's foot anchor, stop horizontal steering and reject jump requests while gravity continues. Start a configurable one-second timer. Returning to light resets the timer and restores movement input. Switching back to Physical is possible if the destination form fits. At zero, restart the level.
- **Finish and restart.** Contact with the exit completes the level. Hazards, falling out of the level, or the R key reset the character position, velocity, form, lamp position and aim, stability timer, and movement state. Pause, failure, completion, and reset gate gameplay input. After a reset or resume, release held mouse buttons before beginning another drag or aim gesture.

### Level beats

See the [first playable greybox layout](Design/first-playable-greybox.drawio.svg) for the side view, position markers, and a light coverage example.

| Beat | What the player learns | Greybox check |
|---|---|---|
| Move and Aim | Control the character while positioning and rotating the light. | A new player moves with A/D, jumps with W, drags with the left mouse button, and aims with the right mouse button. |
| Shift Through | Each form passes a different obstacle. | A Physical Solid requires Shadow. A Shadow Barrier requires Physical. |
| Carry the Light | The spotlight keeps Shadow stable during movement. | The crossing is longer than the lit span. The player moves and aims the lamp while steering the character to reach the exit in light. |

## GitHub starter issues

Use each row as an issue draft. Give every issue one owner and a milestone. Copy the acceptance check into the issue body, then link its dependencies. Split work across separate prefabs or test scenes when several people are editing in Unity.

| Issue title | Suggested owner | Due (UTC+08:00) | Acceptance check | Depends on |
|---|---|---|---|---|
| Design the first playable level | Design lead | 8 Nov | A three-beat sketch, agreed interaction rules, and a must-have asset list are linked in the issue. | — |
| Import a one-screen art sample | Art owner | 8 Nov | Two forms and an obstacle appear at the intended scale in Unity. Sprite import settings and the foot pivot are recorded. | First level sketch |
| Smoke-test an itch.io build | Build owner | 8 Nov | A teammate opens the uploaded test build. Record the tested target and fallback. | Unity project access |
| Build the spotlight rail, aiming, and light footprint | Gameplay owner | 10 Nov | Left-button dragging clamps to the rail, right-button aiming rotates the beam, and the visible cone matches the gameplay check. | [SpotlightRail contract](Architecture/spotlight-rail.md) and [footprint rules](Architecture/light-footprint-and-movement.md) |
| Implement form switching and shadow stability | Gameplay owner | 11 Nov | Test all three obstacle types, a rejected switch inside a wall, a successful recovery after leaving the light, and a failure at timeout. | Collision rules and light footprint |
| Integrate direct movement, jumping, exit, and restart | Main scene owner | 13 Nov | A/D movement, W jumping, and Left Shift work while dragging and aiming. The greybox plays from spawn to exit. Failure and R restore the starting state. | Spotlight and form systems, [movement integration](Architecture/light-footprint-and-movement.md) |
| Deliver the first importable art kit | Art owner | 13 Nov | Two form sprites, three obstacle types, ground/platform, light/rail, a hazard, and an exit can replace placeholders without changing collision. | Art sample and level sketch |

## Production milestones

| Milestone | Programming | Art and design | Exit check |
|---|---|---|---|
| 8 Nov · Kickoff | Test build and named integration owner | Rule decisions, level sketch, art sample | Teammates can open the project and know their first task. |
| 13 Nov · First Playable | Core loop, exit, failure, restart | Readable placeholders and first art kit | The greybox can be completed from a build. |
| 18 Nov · Full Level | All three beats and basic feedback | First art pass and tutorial prompts | Two new players try the level without a walkthrough. Record where they get stuck. |
| 23 Nov · Content Lock | Final asset integration and key sound cues | Full art pass, legible forms and obstacles, final layout | The team completes the level with release visuals. Freeze layout and mechanics. |
| 27 Nov · Release Candidate | Clean clone and LFS check, fresh-device playthrough | Cover, screenshots, credits, English controls | Verify the itch.io page and release build. Identify the source commit and confirm repository access and submission owners. |
| 29 Nov · Submit | Package blocking fixes and identify the final build | Verify page visuals and credits | Submit the itch.io entry, link the public repository, and verify both as a visitor. |

If the 13 November build cannot be completed, keep one flat route and cut layered routes and extra puzzles. Keep the agreed movement, jump, form, and mouse controls in the test build. If the 18 November playtest or art delivery slips, prioritize the light boundary, both forms, obstacle readability, and failure feedback. Use a single background and reusable environment pieces. Defer detailed animation, parallax, and story scenes. If WebGL fails the early build check, use a Windows download as the baseline and keep browser support conditional on a successful playthrough.

## English collaboration

- Write issues, PRs, commits, shared design notes, and player-facing text in English. Use chat for discussion and record the decision and owner in the relevant GitHub issue.
- Every one or two days, post `Done / Next / Blocked`. Include a PR, screenshot, or build link when available. Use explicit time zones in messages with deadlines.
- Follow [the repository workflow](../CONTRIBUTING.md). Open focused PRs, link issues with `Closes #123`, and include verification steps. Add the corresponding `.meta` files and use the existing Git LFS rules for assets. The main scene owner integrates changes into the playable level.

## Kickoff message for the team

> Hi team! Our first target for Shadow Shift is a playable greybox by 13 November. We’re aiming for one short level where players move with A/D, jump with W, and switch forms with Left Shift. The left mouse button drags the lamp along its rail, and the right mouse button aims its beam. Players must keep the shadow lit through the final crossing.
>
> Please reply by 7 November at 12:00 UTC with your time zone, the hours you can realistically commit through 13 November, one task you’d like to own, and any blockers. I’ll handle the first level sketch, starter art sample, and repo setup. We need an owner for gameplay integration and someone to verify the first itch.io build.
>
> We’ll track issues, PRs, and design decisions in English on GitHub. A short Done / Next / Blocked update every day or two is enough. Once we have owners, we can start the gameplay and art work in parallel.
