# UI 1.6.3

Root cause: dungeon roar, trophy and status windows were drawn inside DrawDungeonWorld using the world cover matrix. Cat achievements and attack labels also mixed world and screen coordinates. Those overlays now draw after the interface matrix is applied, independently of camera depth, location and zoom. World-attached enemy labels remain in world space.

All Wood panels now use the shared nine-slice gothic_panel texture, preserving detailed corners at different window sizes. The same frame is exported for HTML touch buttons. Health panel enlarged from 245x107 to 355x148; hearts from 27x31 to up to 43x44; health text from 14 to 22.

Validation includes invariant interface matrices at four route positions, underground height, play/pause/options modes, plus existing input, physics and terrain tests. Physical-phone validation remains unavailable.
