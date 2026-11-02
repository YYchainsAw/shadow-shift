# Contributing

## Development environment

- Use Unity **6000.3.10f1**, matching [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- Install Git and Git LFS. Before cloning the repository, run:

```bash
git lfs install
```

Use English for documentation, issues, pull request descriptions, and commit messages so everyone on the team can follow the work.

## Branches

Create a branch from the latest `main`:

```bash
git switch main
git pull --ff-only
git switch -c feature/player-movement
```

Use the prefix that matches your task:

| Prefix | Purpose | Automatic PR label |
| --- | --- | --- |
| `feature/` | New features | `type:feature` |
| `fix/` | Bug fixes | `type:bug` |
| `refactor/` | Code refactoring | `type:refactor` |
| `docs/` | Documentation | `type:docs` |
| `art/` | Artwork and visual assets | `area:art` |
| `audio/` | Music and sound effects | `area:audio` |

## Commits

Keep each commit focused on one change and describe it clearly. For example:

```text
feat: add player movement
fix: prevent the player from passing through walls
art: add the player idle animation
audio: add collision sound effects
docs: update setup instructions
```

## Pull requests

- Keep each PR focused on one task or issue.
- Target `main` and request a review from [@YYchainsAw](https://github.com/YYchainsAw).
- Explain what changed and how to verify it. If you could not test it, explain why.
- Include screenshots or videos for visual, animation, or audio changes when possible.
- Request another review if you make further changes after approval.
- To close an issue when the PR is merged, include a reference such as `Closes #123`.

The PR milestone workflow inherits a milestone when all linked closing issues belong to this repository and share the same milestone. It leaves an existing PR milestone unchanged. If linked issues have different milestones, choose one manually.

## Unity assets and scenes

- Commit the corresponding `.meta` files when adding, moving, or deleting assets and folders.
- Move and rename assets in the Unity Editor to preserve references.
- Coordinate before editing the same scene or prefab as another teammate.
- Use the existing Git LFS rules in [.gitattributes](.gitattributes) for images, source artwork, and audio.
- Review the changed file list before committing. Keep generated caches, local settings, and unrelated files out of the PR.

## Verification

Before opening a PR, open the project in the required Unity version, check for compilation errors, and test the affected scenes or features. Describe the steps and expected results in the PR.

Automated PR checks validate Unity `.meta` files and Git LFS pointers. They do not build the game or run Unity tests.
