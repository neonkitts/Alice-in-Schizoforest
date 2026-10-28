# Gothic UI integration — 1.7.0

Uses images already generated earlier in this conversation with built-in image_gen (no API fallback):
- ui-window-source.png: exec-4910b22b-40ab-41e0-87a4-127dba86780b.png — carved ebony gothic dialog body, burgundy blank interior, sculpted silhouette.
- ui-kit-source.png: exec-daee7d18-7259-467b-8e7d-aa0d47e994a0.png — bat-wing button, thorn health plaque, pointed inventory card, circular joystick ornament.
- ui-icons-source.png: exec-4e194637-160d-4365-9eef-95889936bbea.png — parchment attack, satchel, pause pillars, hand; chroma-green background.

The source images are preserved. AliceUIImport extracts sprites and removes edge-connected smooth backdrop, and imports icons using chroma key. Runtime panels preserve broad sculpted corners and stretch only the blank central areas; buttons and cards use complete sprites. Mobile controls reuse the same exported PNGs as Unity. New generated variant exec-adb9fffa is not used.
