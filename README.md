# shadowshift

A 2D puzzle platformer set in an abandoned film studio, made for [GitHub Game Off 2026](https://itch.io/jam/game-off-2026) and its theme, **SHIFT**.

Slide and aim a studio spotlight to switch between Physical and Shadow forms. Stay in the beam as Shadow, use each form to get past different obstacles, and use the light to power your way out.

## Play in Unity

Install Git LFS, then clone the project.

```bash
git lfs install
git clone https://github.com/YYchainsAw/shadow-shift.git
cd shadow-shift
git lfs pull
```

1. In Unity Hub, add the cloned folder and open it with Unity **6000.3.10f1**. [Project version](ProjectSettings/ProjectVersion.txt)
2. Open [`MainMenu.unity`](Assets/Scenes/MainMenu.unity).
3. Set the Game view to **16:9** and enter Play Mode. Choose **Play**, then **1** to enter Studio Demo.

Open [`StudioDemo.unity`](Assets/Scenes/StudioDemo.unity) to test the room directly. Choose **Main Menu** when paused or after completing the room to return to the title screen.

## Controls

| Input | Action |
| --- | --- |
| `A` / `D` | Move left or right |
| `W` | Jump |
| `Left Shift` | Switch forms. Enter Shadow while lit |
| Hold and drag the left mouse button | Slide the spotlight along its rail |
| Hold the right mouse button | Aim the beam at the cursor |
| `Esc` | Open the pause menu or resume |
| `R` | Restart the room |

[Demo walkthrough and playtest notes](Docs/Development/first-playable.md#studio-demo-route)

## Game Off submission

This repository contains the Unity source for the jam entry. Game Off requires a public GitHub source repository and a separate playable submission on itch.io. [Official Game Off 2026 rules](https://github.blog/open-source/gaming/game-off-2026-the-theme-is-shift/#how-to-participate)
