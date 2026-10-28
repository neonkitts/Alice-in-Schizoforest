# Mobile UI 1.7.2
- Center inventory/pause visible alpha bounds (original artwork retained).
- Restore header controls to 76 CSS px (64 on narrow viewports).
- Fixed visible-screen geometry for joystick/attack; inverse visualViewport.scale compensation.
- Render pixel ratio captured once rather than changed by later browser zoom.
- Bounded in-memory viewport diagnostics; no transmission.
- Optional ?touch=1 for desktop browser QA of the actual touch interface.
- DOM tests exercise 181 successive viewport changes including 1x, 2x and 1.5x zoom. This is a simulated test, not physical-device verification.

Safari report: controls grow and scene crops during multi-touch. Added non-passive gesturestart/change/end and multi-touch start/move prevention, plus touch-action:none on page and control ancestor. Pointer joystick and attack remain independent. Reverted manual canvas attribute resizing; Unity is the sole render-buffer owner (automatic canvas matching, fixed DPR). BrowserViewport debug entry is unused. Browser mobile viewport override showed inconsistent canvas capture; physical iPhone Safari remains unverified.
