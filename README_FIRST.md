# THE LAST BET — Unity Saloon AI Starter Pack

This ZIP is designed to be extracted directly into the root of your existing **Unity 6** project.

## What is included

- Curated saloon reference images from this ChatGPT design session.
- Transparent modular asset reference sheets where available.
- Male and female gunslinger reference assets.
- Duel scene/UI visual references.
- A production-oriented Unity AI/Cursor master prompt.
- Scene assembly/import instructions.
- Meshy generation/export guidance.
- A small compile-ready Unity C# authoring scaffold for station anchors and validation.
- Empty project folders for models, materials, textures, prefabs and scenes.

## Important limitation

The actual FBX/GLB files created outside ChatGPT (for example in Meshy) were **not uploaded here**, so this pack cannot include those 3D model binaries. Add them under:

`Assets/TheLastBet/Art/Models/Saloon/`

and character files under:

`Assets/TheLastBet/Art/Models/Characters/`

The AI agent should use the reference images to assemble and polish those real 3D assets, not attempt to turn a reference sheet into a runtime texture.

## Install

1. Back up or commit your Unity project first.
2. Extract this ZIP into the Unity project root — the folder that already contains `Assets/`, `Packages/` and `ProjectSettings/`.
3. Open Unity and allow the project to import the files.
4. Add your FBX/GLB models into the model folders above.
5. Give your AI coding/Unity agent the contents of `Docs/01_UNITY_AI_MASTER_PROMPT.md`.
6. Tell the agent to inspect `Assets/TheLastBet/References/` before it changes the scene.
7. Follow `Docs/02_UNITY_IMPORT_AND_ASSEMBLY.md`.

## Absolute modular rules

- The room shell is architecture only.
- The **bar/back-wall module** stays separate and fills the rear opening.
- The **entrance/welcome gate** stays separate and fills the front opening.
- Poker, roulette, slots, central lounge, duel arena and gallows are separate prefabs.
- Do not duplicate geometry already included by those prefabs.
- Central lounge: **no chandelier**.
- Keep the selected decorative plants from the final shell reference; do not add plants into the bar/entrance gaps where their dedicated models need to fit.
- Keep game rules separate from environment scripts.

Start with `Docs/00_ASSET_MAP.md` and `Docs/01_UNITY_AI_MASTER_PROMPT.md`.
