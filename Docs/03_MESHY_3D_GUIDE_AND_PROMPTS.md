# Meshy 3D Generation Guide

Use the transparent multi-angle sheets in `Assets/TheLastBet/References/Saloon/Modules/` as visual references when Meshy supports reference-image workflows.

## General target prompt

> Production-ready modular 3D asset for a premium late-1800s Wild West saloon game. Stylized realism, realistic proportions, aged dark wood, worn leather, brass and iron hardware, believable construction, clean topology, PBR materials, game-ready UVs, readable from an elevated 3/4 camera, no background plane, no unrelated scenery, no baked fake shadows in base color, no duplicate floating parts. Preserve the proportions and design of the reference image. Produce a complete object with sensible unseen/back-side geometry.

## Room shell

> Modular Wild West saloon room shell only. Wooden plank floor, perimeter walls, paneling, structural posts, beams, windows, wall trim and selected wall decoration. Rear-center section must be open for a separate full bar module. Front-center section must be open for a separate entrance/gate module. Keep side partitions matching the reference. No bar, no poker tables, no roulette, no slot machines, no duel ring, no gallows, no lounge furniture, no chandelier. Preserve only non-conflicting architectural decoration and the selected plants shown by the final room-shell reference.

## Central lounge

> Modular central saloon lounge: red western patterned rug, round aged wood table, four premium western leather armchairs using red and dark-green upholstery, decanter/bottle, glasses and papers/cards, low decorative rail/posts and small lamps/plants matching reference. NO chandelier. Isolated game-ready asset with clean underside/back geometry.

## Export

Keep both when practical:
- FBX for Unity production interchange
- GLB as a portable backup/reference

Prefer separate PBR textures where possible. Avoid baking scene lighting directly into BaseColor.
