# 1.6.2 — rendering and touch corrections

- Holding the joystick upwards or diagonally keeps the jump buffer active, so landing automatically starts the next jump. Release cancels further queuing; keyboard jumping remains unchanged.
- Canvas fills the complete browser viewport on desktop and mobile. Unity uses a uniform cover scale for the world, with the ground anchored at 76% of screen height during gameplay. UI fits independently inside safe bounds. No CSS stretching or reserved black side strips.
- One poster is shown during loading. The transparent mobile start hit area has an accessible label but no second visible instruction. A tap skips the subsequent Unity splash once loading completes.
- Terrain tiles use a stable world origin and shared rounded endpoints. Mobile terrain textures use bilinear filtering and mipmaps to reduce minification shimmer. Desktop texture filtering stays pixel-sharp.
- Eight separate crouch PNG sprites replace the procedural strip deformation: entry, idle and four low-walking poses, all anchored at the feet.
- Touch controls use the game's pixel font, burgundy ornamental frames, a thorn-ring joystick and the actual analysis-paper icon.

Browser/device visual testing still requires confirmation on physical devices. Automated input, collision and seam checks are recorded in the build logs.

Passed: 141302 baseline assertions; 1000 moving-camera shared-edge comparisons; 7 consecutive jumps during a five-second held diagonal; 8 distinct imported PNG frames. Web template input/full-viewport tests passed at 844×390, 932×430 and 1024×768. Both Unity builds succeeded. Vercel deployment dpl_21yrnzMCnxq8En18U5aM3cWLNue1 is READY at https://alice-in-schizoforest.vercel.app.
