# Shadow Shift documentation

## Start here

- [Development plan](DevelopmentPlan.md) covers milestones, team roles, and release checks.
- [First playable greybox](Design/first-playable-greybox.drawio.svg) shows the three level beats, position markers, and spotlight timing for the final crossing.
- [Prototype use-case diagram](Architecture/UML/shadow-shift-use-cases.drawio.svg) maps player and level designer interactions for the direct-control design confirmed on 11 November.
- [Character locomotion state machine](Architecture/UML/character-locomotion-state-machine.drawio.svg) specifies A/D movement, W jumping, airborne steering, and landing for the first playable greybox.
- [Character form state machine](Architecture/UML/character-form-state-machine.drawio.svg) specifies Left Shift requests, collision guards, and shadow stability feedback.
- [Level session state machine](Architecture/UML/level-session-state-machine.drawio.svg) specifies pause, failure, completion, and the shared reset flow.
- [SpotlightRail integration contract](Architecture/spotlight-rail.md) covers left-button dragging, right-button aiming, and the handoff to the light footprint.
- [Light footprint and direct movement](Architecture/light-footprint-and-movement.md) defines light sampling, movement input, and the greybox integration checks.
- [Repository workflow](../CONTRIBUTING.md) covers branches, pull requests, and Unity asset changes.

## Where documents belong

| Path | Content |
| --- | --- |
| `Docs/` | Documentation index and development plan |
| `Docs/Design/` | Gameplay rules, level layouts, and art specifications |
| `Docs/Architecture/` | System boundaries, technical designs, and decisions |
| `Docs/Architecture/UML/` | Use-case, state machine, sequence, and class diagrams |
| `Docs/Development/` | Implementation guides, test procedures, build instructions, and release checklists |

Create a directory when it has its first document. Write in English, use descriptive lowercase filenames with hyphens for new topic files, and link to them with relative paths. Add each new topic to this index.

## Working with diagrams

- Keep one editable `.drawio.svg` file per diagram. Open it in draw.io to edit the embedded model. On export, select SVG and enable **Include a copy of my diagram** so the result remains editable and can be previewed in GitHub.
- Name the file after its subject and diagram type, such as `character-locomotion-state-machine.drawio.svg`. Update the diagram in the same pull request as the behavior it describes.
- Use-case diagrams describe the goals of the people using the system. Record transition triggers, guards, and effects in state machine diagrams.

## State machine reference

The earlier [2D RPG project](https://github.com/YYchainsAw/2D-RPG-250.01) provides a familiar `Enter`, `Update`, and `Exit` lifecycle. Model character locomotion, character form, and the level session separately. A/D and W feed locomotion, Left Shift feeds the form state machine, and the lamp's position and rotation feed the light footprint and shadow stability checks.
