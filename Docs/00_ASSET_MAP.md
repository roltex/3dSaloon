# Reference Asset Map

The AI agent should inspect these references before changing the saloon scene.

| File | Purpose | Alpha |
|---|---|---|
| `00_Final_Saloon_Composition.png` | Final target composition; transparent background; use for overall layout, palette and mood. | Yes |
| `01_Room_Shell_Final_Reference.png` | Final room-shell reference sheet: modular openings and selected plants retained. Do not treat as a texture atlas. | No |
| `02_Bar_Transparent_Asset_Sheet.png` | Bar module reference; includes bar architecture, stools and details; alpha background. | Yes |
| `03_Entrance_Transparent_Asset_Sheet.png` | Entrance/welcome gate module; alpha background. | Yes |
| `04_Green_Poker_Table_Transparent.png` | Green poker table module and angles; alpha background. | Yes |
| `05_Red_Round_Card_Table_Transparent.png` | Red round card table module and angles; alpha background. | Yes |
| `06_Roulette_Transparent.png` | Roulette module and close-ups; alpha background. | Yes |
| `07_Slot_Machines_Transparent.png` | Jackpot/slots module; alpha background. | Yes |
| `08_Central_Lounge_No_Chandelier_Transparent.png` | Central lounge module; no chandelier; alpha background. | Yes |
| `09_Duel_Arena_Transparent.png` | Duel arena module and multiple angles; alpha background. | Yes |
| `10_Gallows_Judgment_Transparent.png` | Gallows/judgment area module; alpha background. | Yes |
| `11_Small_Props_Transparent.png` | Signs, cards, bottles, barrels, crates, plants and small props; alpha background. | Yes |
| `12_Seating_Lighting_Props_Transparent.png` | Seating, table and lighting prop references; alpha background. | Yes |
| `20_Male_Gunslinger_Transparent.png` | Male character front cutout; transparent background. | Yes |
| `21_Female_Gunslinger_Transparent.png` | Female character front cutout; transparent background. | Yes |
| `22_Male_Turnaround_And_Breakdown.png` | Male turnaround, parts and material references. | No |
| `23_Female_Turnaround_And_Breakdown.png` | Female turnaround, hair, outfit and material references. | No |
| `24_Male_Equipment_And_Accessories.png` | Male equipment/accessory reference sheet. | No |
| `25_Female_Equipment_And_Materials.png` | Female outfit/material detail reference sheet. | No |
| `30_Duel_Arena_InWorld_Reference.png` | In-world 3D duel arena with two characters; use for scene staging. | No |
| `31_Duel_UI_Steady_Reference.png` | Duel UI/state reference for STEADY phase. | No |
| `32_Duel_UI_Result_Reference.png` | Duel result/victory state reference. | No |

## Authority order

1. **Actual FBX/GLB models in the project** — geometry authority.
2. `00_Final_Saloon_Composition.png` — overall composition, visual language, lighting and station placement.
3. `01_Room_Shell_Final_Reference.png` — architectural-shell intent and modular openings.
4. Individual module sheets — shape, materials, details and interaction-part breakdown.
5. Character and duel sheets — character setup and duel presentation.

Do not use the reference sheets as final in-game texture atlases. They are concept/production references.
