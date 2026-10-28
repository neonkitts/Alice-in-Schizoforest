# Mobile controls 1.6.1

- Transparent pointer-captured joystick; horizontal deadzone 0.20, jump threshold +0.48, crouch threshold -0.42. Diagonals combine actions; jump triggers once per upward gesture.
- Attack remains on the right. Rescue appears only near the kitten. Inventory and pause are above the scene.
- Pointer cancellation, focus loss, orientation changes, dialogue and pause reset input. A second finger cannot steal the joystick.
- Mobile world scale 1.38; desktop scale unchanged. Pickup notifications sit below the ground baseline.
- Crouch has entry/exit interpolation, idle breathing and a moving shuffle using the existing pixel sprite. Forest and dungeon share the renderer.

Passed isolated JavaScript handler tests: diagonal motion, attack multitouch, cancellation, pointer ownership, pause/dialogue reset, focus loss and contextual rescue. Run with `node Tools/test_mobile_controls.cjs`.

Browser visual testing was blocked by the browser tool's unavailable admin-policy security check. No physical Android/iOS device was available; real-device visual and touch testing remains unverified.
