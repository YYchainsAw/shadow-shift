# Shadow Shift documentation

## Start here

- [Development plan](DevelopmentPlan.md) covers milestones, team roles, and release checks.
- [First playable greybox](Design/first-playable-greybox.drawio.svg) shows the three level beats, waypoint positions, and spotlight timing for the final crossing.
- [Prototype use-case diagram](Architecture/UML/shadow-shift-use-cases.drawio.svg) maps player and level designer interactions from the Technical Game Design Document v0.1.
- [Character locomotion state machine](Architecture/UML/character-locomotion-state-machine.drawio.svg) specifies movement, route changes, and landing for the first playable greybox.
- [Character form state machine](Architecture/UML/character-form-state-machine.drawio.svg) specifies shift guards, denial feedback, and when to recalculate the route.
- [Level session state machine](Architecture/UML/level-session-state-machine.drawio.svg) specifies pause, failure, completion, and the shared reset flow.
- [SpotlightRail integration contract](Architecture/spotlight-rail.md) covers the standalone pointer test and the handoff to the light footprint and route planner.
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

The earlier [2D RPG project](https://github.com/YYchainsAw/2D-RPG-250.01) provides a familiar `Enter`, `Update`, and `Exit` lifecycle. Model character locomotion, character form, and the level session separately. The spotlight feeds the route planner, which supplies movement targets to the locomotion state machine.
