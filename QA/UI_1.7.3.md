# 1.7.3
- Repair low ceiling/ground obstacle combinations with 3.6 world units transition clearance. Fixture reproduces the supplied screenshot, plus checks across 100 seeded maps.
- Kitten prompt shortened and inset inside plaque; vertical position follows ground.
- Desktop inventory and pause draw cropped alpha bounds at ring centre. No text under pause.
- Cave renderer includes shared forest enemies and attack paper: previously camera entering transition could hide still-living enemies.
- Knockback no longer inserts an enemy into solid terrain. Regression checked with the actual Attack method.
- Includes Safari two-finger page-zoom prevention and mobile icon centring from 1.7.2.

Shared PaperAttackOffset follows a downward world-space path on desktop/mobile, mirrored in x only. 21 trajectory samples tested. Windows build and runtime smoke pass (inventory, kitten, castle, dungeon).
