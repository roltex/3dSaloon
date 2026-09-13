# Unity 6 Import & Saloon Assembly Guide

## 1. Add your actual 3D files

Put saloon models in:

`Assets/TheLastBet/Art/Models/Saloon/`

Put character models in:

`Assets/TheLastBet/Art/Models/Characters/`

Recommended source format for Unity production: **FBX**. Keep **GLB** copies if that is what Meshy exports cleanly, but convert/verify them in your normal DCC pipeline if Unity import/material behavior is inconsistent.

## 2. Model import settings

For each environment mesh:
- verify scale against a 1.75–1.85 m character
- read/write: off unless required by a runtime operation
- generate lightmap UVs only when the model does not already contain a good second UV set and you intend to bake lighting
- normals: Import where the source normals are good; calculate only if necessary
- tangents: calculate/import according to material needs
- enable mesh compression only after checking artifacts

For characters:
- Rig: Humanoid when compatible
- Avatar definition: create from this model, or copy from a compatible shared skeleton
- verify T-pose / avatar mapping
- root motion should be decided per animation, not blindly enabled

## 3. Texture/PBR mapping

Reference sheets are not runtime texture atlases.

Prefer actual PBR texture sets:
- BaseColor/Albedo
- Normal
- Metallic
- Roughness or Smoothness
- AO
- Emission where needed

Unity uses **Smoothness** in common Lit workflows, which is the inverse of Roughness:

`Smoothness = 1 - Roughness`

Depending on the active pipeline, smoothness may be read from a texture alpha/channel. Configure this deliberately instead of plugging a roughness map blindly into the wrong slot.

Mark normal maps as `Normal map` texture type.

## 4. Prefab creation

Create these prefabs after model import:

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

Keep visual children below a stable prefab root at local position/rotation zero when possible.

## 5. Room-shell rule

The shell must keep the back-center and front-center modular areas open:

- rear opening = BAR module
- front opening = ENTRANCE module

Do not leave hidden duplicate walls inside these openings. They cause clipping, z-fighting and bad navigation.

Retain the decorative plants shown on the final shell reference, except any plant that conflicts with a module's insertion gap.

## 6. Layout

Use `00_Final_Saloon_Composition.png` as the target, not as a literal top-down texture.

```text
BACK / NORTH
Green Poker       Bar       Red Card Table
Roulette       Central Lounge       Slots
Duel Arena                         Gallows
                  Entrance
FRONT / SOUTH
```

## 7. Station anchors

On each gameplay station:

1. Add `InteractableStation` to a stable root.
2. Add a trigger collider or separate interaction trigger child.
3. Create child transforms:
   - `Anchor_Player`
   - `Anchor_Opponent` when needed
   - `Anchor_Camera`
   - `Anchor_Audio`
   - `Anchor_Spectator_##` as needed
4. Assign them to the component.

## 8. Duel arena

Add `DuelArenaAnchors` to the duel root and create:

- `Duel_PlayerA`
- `Duel_PlayerB`
- `Duel_Camera_Intro`
- `Duel_Camera_Gameplay`
- `Duel_Camera_ResultA`
- `Duel_Camera_ResultB`

The player transforms should face each other.

## 9. Lighting

Build the lighting from practical fixtures. Do not flood the room with one giant orange light.

Use multiple warm sources with controlled ranges, plus indirect lighting/probes appropriate to the active render pipeline. Keep corners darker while ensuring gameplay faces and tables are readable.

## 10. Validation

After assembly:

- run the game in Play Mode
- inspect the saloon from the primary camera
- walk a character through every aisle
- check collider snagging
- check door/bar gap overlaps
- check no chandelier exists in the central lounge
- verify station camera anchors do not clip walls
- verify character feet touch floor
- inspect material normal/roughness behavior
- run Unity menu: `The Last Bet > Validate Saloon Scene`
