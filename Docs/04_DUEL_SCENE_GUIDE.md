# Duel Scene / Presentation Guide

Visual references:

- `References/Duel/30_Duel_Arena_InWorld_Reference.png`
- `References/Duel/31_Duel_UI_Steady_Reference.png`
- `References/Duel/32_Duel_UI_Result_Reference.png`

## Scene intent

The duel should still feel physically inside the saloon, not like a separate generic menu. Use the actual 3D duel arena and characters, then overlay the duel UI.

## Recommended state flow

1. **Enter** — transition/blend from main saloon camera.
2. **Set positions** — competitors at PlayerA / PlayerB anchors.
3. **Steady** — both characters in tense idle; gun remains holstered.
4. **Cue** — random legal draw signal from authoritative gameplay logic.
5. **Draw** — animation begins.
6. **Fire** — validated shot fires.
7. **Resolve** — impact/fall or miss.
8. **Result** — winner remains readable; loser pose/fall; UI result.
9. **Exit** — return camera/gameplay to saloon.

## Presentation

- warm saloon lighting on both characters
- slightly darker audience/background
- shallow depth-of-field only if it does not harm input/readability
- strong silhouette separation
- subtle dust kick-up on impact/fall
- muzzle flash must be brief and not obscure UI
- do not fake the loser by simply setting opacity to 20%; use actual animation/pose when models are rigged

## Gameplay separation

Environment components hold anchors only. The authoritative duel rules, timing, early-shot handling, winner selection and networking should remain in gameplay/server systems.
