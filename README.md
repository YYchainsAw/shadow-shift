<div align="center">
  <img src="Assets/Art/Branding/itch-cover.jpg" alt="shadowshift cover artwork" width="640" />

  <h1>shadowshift</h1>

  <p><strong>A 2D spotlight puzzle platformer set in an abandoned film studio, built for GitHub Game Off 2026 (Theme: SHIFT).</strong></p>

  <p>
    <a href="https://yychainsaw.itch.io/shadow-shift">
      <img src="https://img.shields.io/badge/Play%20on-itch.io-6be2f0?style=for-the-badge&logo=itch.io&logoColor=131027&labelColor=1c1a3a" alt="Play on itch.io" />
    </a>
    <a href="https://itch.io/jam/game-off-2026">
      <img src="https://img.shields.io/badge/GitHub%20Game%20Off-2026%20%C2%B7%20SHIFT-9e7be8?style=for-the-badge&labelColor=1c1a3a" alt="GitHub Game Off 2026" />
    </a>
    <a href="LICENSE">
      <img src="https://img.shields.io/badge/License-MIT-f4efe6?style=for-the-badge&labelColor=1c1a3a" alt="MIT License" />
    </a>
  </p>
</div>

---

## About the Game

Trapped on the dark soundstage of an abandoned film studio, you control **both the performer on stage and the overhead spotlight at the same time**.

- **Shift Your Form (`Left Shift`)** — Step into the beam to become **Shadow**, passing through solid blue barriers and riding illuminated updrafts with a slow, weightless descent. Shift back to **Physical** to pass violet shadow barriers, press heavy floor switches, and smash through cracked floors from high above.
- **Shift the Light (`Mouse`)** — Slide the studio lamp along its rail and rotate its beam in real time while moving and jumping. Solid studio structures cast real shadows that block illumination—and if your Shadow leaves the beam, you only have a brief stability window before fading out.
- **Diegetic Guidance** — Permanent circuit conduits connect every switch and battery to what it powers, lighting up with traveling pulses when activated, paired with clean overhead status cues instead of walls of text.

### Theme Interpretation — SHIFT

We interpreted **SHIFT** on three intertwined levels: shifting your character between **Physical** and **Shadow** forms (literally triggered with `Left Shift`), shifting the **studio spotlight** along its rail and rotating its cone to reshape lit paths and shadows, and shifting **lamp control** from the first spotlight to a powered second spotlight in the final stage.

---

## Preview

<div align="center">
  <img src="Docs/Media/main-menu.png" alt="shadowshift main menu" width="760" />
</div>

### Three Progressive Soundstages

1. **Stage 1 · Light & Form** (`StudioLevel1`) — Compact onboarding room introducing spotlight aiming, form shifting through blue barriers, dragging the lamp along its rail, and returning to Physical for violet barriers.
2. **Stage 2 · Wind & Conduit** (`StudioLevel2`) — Two-tier room pairing Physical/Shadow color-coded switches with permanent circuit wires, illuminated updrafts, and stage checkpoints.
3. **Stage 3 · Shatter & Charge** (`StudioDemo`) — Full-hall finale combining switches, wind ascent, high-impact floor breaking, light occlusion, 1.25 s accumulated battery charging, and a second-spotlight handoff to reach the exit.

---

## Controls

| Input | Action |
| --- | --- |
| `A` / `D` | Move left or right |
| `W` | Jump |
| `Left Shift` | Switch between Physical and Shadow *(feet must be lit to enter Shadow)* |
| Hold & drag Left Mouse | Slide the active spotlight along its rail |
| Hold Right Mouse | Aim the spotlight beam at the cursor |
| `R` | Restart the current stage |
| `Esc` | Pause or resume |

---

## Run from Source

Built with **Unity `6000.3.10f1`** (URP 2D, uGUI, and Input System). See [`ProjectVersion.txt`](ProjectSettings/ProjectVersion.txt) and [`manifest.json`](Packages/manifest.json).

```bash
git lfs install
git clone https://github.com/YYchainsAw/shadow-shift.git
cd shadow-shift
git lfs pull
```

1. Open the cloned folder in **Unity Hub** using **Unity 6000.3.10f1**.
2. Open [`Assets/Scenes/MainMenu.unity`](Assets/Scenes/MainMenu.unity).
3. Set the Game view aspect ratio to **16:9**, enter Play Mode, and click **Play** to choose Stage **1**, **2**, or **3**.

You can also open [`StudioLevel1.unity`](Assets/Scenes/StudioLevel1.unity), [`StudioLevel2.unity`](Assets/Scenes/StudioLevel2.unity), or [`StudioDemo.unity`](Assets/Scenes/StudioDemo.unity) directly. See the [stage walkthrough and development docs](Docs/Development/first-playable.md#studio-demo-route) for architecture and route details.

---

## Credits & License

- **Design, Programming & Art:** [YYchainsAw](https://github.com/YYchainsAw)
- **Sound Effects:** [Kenney](https://kenney.nl) (*Interface Sounds*, *Impact Sounds*, *Music Jingles*) under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)
- **Source Code License:** Released under the [MIT License](LICENSE)
