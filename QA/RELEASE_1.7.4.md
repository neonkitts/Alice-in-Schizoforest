# 1.7.4 — music and route pacing only
Original MainTheme.mp3 is byte-identical to D:/Alisa shiza main 1.mp3.
DemonChase, AnxietyDemon, GymDemon, TattooRoar, BranchCrunch and MonsterDefeat are byte-identical to the 1.7.3 source archive. Music switching code unchanged.
New route: spawn → doctor → kitten → castle; castle exit → cave; cave exit → home. Every ordinary route leg is 306 world units / 5.1 speed = 60 seconds. Obstacle handling, stops and battles add time.
Castle/cave terrain generation and encounters unchanged; random terrain preserved. Existing long-route saves regenerate their original map. New-route saves normalize generation to the same canonical map length before placing terrain, so reload is deterministic.
Windows checks: 100 seeds, mobile checks, persistence checks pass.

Windows runtime smoke: PASS inventory=4-lives kitten=rescued castle=escaped dungeon=rewarded. Main clip loaded (81.86775 seconds), chase switch and background return logged. WebGL pre-build validation: 99860 checks, 1000 mobile terrain checks, 7 persistence checks passed.
