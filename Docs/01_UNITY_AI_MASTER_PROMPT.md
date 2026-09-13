# COPY/PASTE PROMPT — UNITY AI / CURSOR / AGENT

You are the lead Unity 3D environment artist, technical artist, level designer and scene engineer for **THE LAST BET**.

## First action — inspect project and references

Before changing anything:

1. Inspect the existing Unity project structure, scenes, prefabs, models, materials, textures, scripts and render pipeline.
2. Inspect **every image** under `Assets/TheLastBet/References/`.
3. Read `Docs/00_ASSET_MAP.md` and `Docs/02_UNITY_IMPORT_AND_ASSEMBLY.md`.
4. Search for real FBX/GLB models before creating replacement geometry.
5. Reuse existing working systems. Do not delete or rewrite unrelated code.
6. Make a short implementation plan, then execute it.

## Engine / target

- Unity 6
- C# only for gameplay/editor code
- PC / Steam target
- Real 3D scene; never a flat 2D background
- Use the render pipeline already configured by the project. If the project has not selected one and it is desktop-only, HDRP is acceptable for the premium target; otherwise do not force a pipeline migration.

## Primary visual target

Create a premium late-1800s Wild West saloon that feels warm, expensive, worn, dangerous and cinematic.

Art direction:
- stylized realism, not cartoon
- rich aged dark wood and visible wood grain
- worn red / brown / dark-green leather
- aged brass and iron hardware
- red velvet accents
- dusty floor and accumulated wear
- warm amber lantern pools with darker spaces between stations
- subtle haze/dust in the air
- elegant gambling house + outlaw hideout
- readable from a high 3/4 gameplay camera
- no sterile modern surfaces
- no bright mobile-game look
- no excessive bloom

Atmosphere rule:
**Make the saloon warm enough that players want to stay there, luxurious enough that gambling feels important, and dangerous enough that every shadow suggests someone may be about to betray, challenge or eliminate another player.**

## Reference authority

`00_Final_Saloon_Composition.png` defines the target final composition, palette and atmosphere.

`01_Room_Shell_Final_Reference.png` defines the base architectural shell.

The following references define **separate modular gameplay objects**:
- bar
- entrance/welcome gate
- green poker table
- red round card table
- roulette
- slot-machine/jackpot area
- central lounge
- duel arena
- gallows/judgment area

If matching FBX/GLB assets already exist, use those assets. Reference images are art guidance; actual imported models are geometry authority.

## Critical modular geometry rules

The base room shell must contain only architecture and non-conflicting decoration:
- floor
- perimeter walls
- wall paneling
- structural posts / beams
- trim
- windows
- wall lantern positions
- selected wall art / skull / small decoration
- architectural partitions
- selected decorative plants shown in the final room-shell reference

The room shell must **not** permanently contain:
- complete bar or bar back-wall
- entrance gate / welcome porch
- poker tables/chairs belonging to their module
- roulette
- slots
- duel arena
- gallows
- central lounge furniture
- chandelier

Leave the rear/top center open for the dedicated BAR module.
Leave the front/bottom center open for the dedicated ENTRANCE module.
Do not hide these gaps with duplicate walls, doors or decorative plants.

Central lounge: **NO CHANDELIER**.

## Target layout

```text
                 NORTH / BACK

       [ GREEN POKER ]   [ BAR ]   [ RED CARD TABLE ]


       [ ROULETTE ]  [ CENTRAL LOUNGE ]  [ SLOTS ]


       [ DUEL ARENA ]                  [ GALLOWS ]


                  [ ENTRANCE ]

                 SOUTH / FRONT
```

Maintain clear player circulation and strong visibility from the gameplay camera.

## Camera

Build an elevated 3/4 isometric-like camera rig:
- perspective camera, not a flat 2D image
- approximately 35–50 degree downward pitch as an initial target
- modest perspective distortion
- full-room readability
- character silhouettes remain readable

Create reusable transforms / `CameraAnchor` objects for:
- main saloon
- bar
- green poker
- red card table
- roulette
- slots
- central lounge
- duel intro
- duel gameplay
- duel result A/B
- gallows
- entrance

Do not hardcode world-space camera coordinates inside game rules.

## Major prefabs

Create or reuse these prefab roots:

- `PF_SaloonShell`
- `PF_Bar`
- `PF_Entrance`
- `PF_PokerGreen`
- `PF_PokerRed`
- `PF_Roulette`
- `PF_SlotArea`
- `PF_CentralLounge`
- `PF_DuelArena`
- `PF_Gallows`

Every gameplay station should be separately selectable, movable, optimizable and interactable.

## Interactable station authoring

Use `InteractableStation.cs` supplied in this pack as the environment authoring component or improve it without moving gameplay win/loss logic into the environment.

Each station should expose as appropriate:
- stable station ID
- player interaction trigger
- player anchor
- opponent anchor
- camera anchor
- spectator anchors
- audio anchor
- collider root

## Bar

Visual centerpiece of the back wall:
- curved aged wooden counter
- brass rail/details
- red stools
- bottle shelving and glassware
- small lamps
- red curtains
- glowing/emissive BAR sign
- rich detailed cabinetry

The bar's own back-wall architecture belongs to the bar prefab; do not duplicate a base wall behind it if it causes overlap/z-fighting.

## Poker / card tables

Green poker:
- green felt
- dark carved wood
- brass cup holders/details
- red leather chairs
- cards/chips as separate child objects when practical

Red round card table:
- red felt
- visually distinct geometry from the green table
- cards/chips/cup holders
- red leather seating

## Roulette

Separate child objects for:
- wheel
- ball
- table/body
- betting surface
- chairs
- chips

The wheel must be able to rotate independently later.

## Slots

Approx. three western slot machines:
- wood/aged metal cabinets
- purple/red illumination
- separate reels, lever and buttons where practical
- stools
- emissive JACKPOT sign

Keep each machine independently interactable.

## Central lounge

Use the supplied lounge reference/model:
- large red patterned rug
- round wood table
- four leather armchairs, red/green mix
- bottle/decanter, mugs, cards/papers
- surrounding low rails/posts if part of the asset
- small lamps / selected plants where reference shows them
- **NO CHANDELIER**

## Duel arena

Bottom-left area:
- rope-fenced indoor western duel ring
- worn dusty floor / red central star
- `DUEL / HONOR OR DUST` sign
- barrels/crates/lanterns
- two opponent anchors facing each other
- intro/gameplay/result camera anchors
- spectator anchors

Gameplay-ready animation flow support:
1. move/blend camera into duel arena
2. place competitors
3. duel idle
4. STEADY state
5. random legal draw cue
6. draw + fire
7. hit/fall reaction
8. winner pose
9. result camera/UI
10. return to saloon

Do not implement authoritative win logic inside the environment prefab.

## Gallows / judgment area

Bottom-right:
- raised wooden platform
- stairs
- heavy beam structure
- separate rope/noose object
- barrels/crates/lanterns
- `JUSTICE FINDS EVERYONE` sign
- character placement point
- cinematic camera anchor

## Entrance

Separate module that fills the front-center opening:
- western gate/swing-door character
- steps
- posts
- metal reinforcement
- lanterns
- WELCOME mat/signage
- only the plants/decoration actually present on the entrance prefab

No duplicate doorway wall behind this module.

## Lighting

Warm practical-driven lighting, initially around 2200–3000K where Kelvin controls are available:
- wall lanterns
- bar lamps/sign
- slot/jackpot emission
- subtle window contribution
- soft contact shadows
- indirect bounce / probes / baked GI where appropriate

Brightness hierarchy:
- bright: bar, lounge focus, duel sign, slot/jackpot
- medium: tables, roulette, entrance
- darker: corners, non-gameplay wall edges and passages

Use subtle volumetric haze if supported. Avoid uniform brightness and excessive bloom.

## Materials

Use proper pipeline-compatible Lit/PBR materials.

Wood:
- aged oak/walnut feel
- grain direction makes physical sense
- scratches, edge wear, varied roughness

Leather:
- red/brown/green
- stitching, creasing, worn polished contact areas

Metal:
- aged brass / iron / steel
- subtle oxidation and edge wear

Fabric:
- red curtains
- western rugs

Floor:
- aged planks
- subtle dirt/wear variation near traffic and walls

## Scale

Use approximately `1 Unity unit = 1 meter`.
Validate scale against the supplied adult humanoid characters.
Do not let furniture or doors feel miniature or oversized.

## Navigation / collision

- Configure walkable surfaces for player movement.
- Preserve comfortable circulation between stations.
- Prefer simple compound colliders over expensive mesh colliders on decorative props.
- Keep collision roots separate from purely visual small details when helpful.
- Do not let chairs/table corners block critical routes unnecessarily.

## Suggested scene hierarchy

```text
SaloonScene
├── Environment
│   ├── SaloonShell
│   ├── Lighting
│   ├── ReflectionProbes
│   └── Audio
├── GameplayStations
│   ├── BarArea
│   ├── GreenPoker
│   ├── RedPoker
│   ├── Roulette
│   ├── SlotArea
│   ├── CentralLounge
│   ├── DuelArena
│   ├── Gallows
│   └── Entrance
├── Navigation
├── CharacterSpawns
├── CameraRig
├── Gameplay
└── UI
```

Do not place hundreds of unrelated objects at scene root.

## Performance target

Commercial PC/Steam scene, not a demo screenshot:
- shared materials when sensible
- GPU instancing where appropriate
- LOD groups for complex assets
- light baking / mixed lighting when appropriate
- reflection probes
- occlusion culling where it helps
- sensible shadow distance and light counts
- avoid unnecessary 4K textures on tiny props

Hero assets: 2K–4K as justified.
Small props: 1K–2K generally.

## Audio authoring anchors

Prepare positions for:
- room ambience / crowd murmur
- lantern/fire
- wood creaks
- bottles/glass
- cards/chips
- roulette
- slot machines
- footsteps
- exterior wind
- duel gunshot

Use spatial audio for local sources.

## Character integration

Use the male/female references to validate scale and lighting.
For real models:
- Humanoid rig when feasible
- consistent root scale/orientation
- feet on floor
- weapon/holster attachment bones or sockets
- shared/compatible animation strategy if the characters are intended to reuse animations

## Final execution order

1. Inspect existing project and references.
2. Locate/import real 3D models.
3. Configure model scale, normals, materials and textures.
4. Build/reuse the modular room shell.
5. Place all station prefabs to match `00_Final_Saloon_Composition.png`.
6. Fix overlaps at bar and entrance openings.
7. Place lounge with no chandelier.
8. Configure lighting, GI/probes and post processing.
9. Configure camera rig and anchors.
10. Configure simple colliders and navigation.
11. Add `InteractableStation` authoring components/anchors.
12. Configure duel anchors/cameras.
13. Enter Play Mode and test camera, scale, collisions and navigation.
14. Fix z-fighting, clipping, wrong normals, oversized lights and bad material mapping.
15. Run `The Last Bet > Validate Saloon Scene` in the Unity editor menu.
16. Leave the scene and prefabs clean and saved.

## Do not

- Do not stop at greybox if final models are already available.
- Do not merge the entire saloon into one mesh.
- Do not rebuild existing 3D assets from scratch without first searching the project.
- Do not duplicate the bar wall or entrance wall behind their modular models.
- Do not add a chandelier to the central lounge.
- Do not move authoritative gameplay rules into environment scripts.
- Do not make the scene a flat image plane.
- Do not destroy working project systems to force your preferred architecture.

## Completion report

When finished, report:
- scene(s) created/modified
- prefabs created/modified
- existing models reused
- scripts added/modified
- lighting/render-pipeline work performed
- navigation/collider work performed
- missing assets
- manual-art issues still remaining
- any warnings/errors still present in the Unity Console
