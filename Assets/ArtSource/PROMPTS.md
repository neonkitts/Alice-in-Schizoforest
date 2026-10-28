# Изображения «Алиса в шизолесье»

Созданы встроенным инструментом imagegen, не CLI. Итоговые изображения сохранены в этой папке и импортируются в `Assets/Resources/Art` средствами Unity с Point-фильтрацией. Технический одноцветный зелёный фон удаляется импортёром; финальные игровые текстуры имеют прозрачность.

- `fairy-sheet-original.png`: sprite atlas on solid neon green, 4 × 2 cells; adult gothic girl, burgundy hair, black oversized hoodie and shorts, chunky black-white sneakers; idle, two run frames, jump, crouch, paper attack, hurt, sleep; crisp 16-bit pixel art.
- `forest-original.png`: gothic forest panorama, twisted dark trees, burgundy leaves, mist, distant ruined arches, muted crimson moon; charcoal/gray/burgundy palette; no UI or characters.
- `enemies-original.png`: four poses of a tall anxiety demon with antlers, ivory mask, thorn ribs and burgundy cloak; four beetle poses; chroma green background, detailed pixel art.
- `tiles-original.png`: 3 × 2 atlas: burgundy earth, gray gothic masonry, root-entangled stone, mire tendrils, two elixir bottles, dead plants and red mushrooms; flat chroma green background.
- `home-original.png`: ancient crooked gray-brown tree, sparse burgundy leaves, glowing doorway-shaped hollow and tangled roots; centered whole-tree pixel-art prop on chroma green.
- `ground-original.png`: full-bleed horizontally tileable terrain strip, continuous burgundy moss top and dark rooted soil; no isolated blocks, borders, rounded corners or vertical joints. Added after the request for a continuous floor.

Промпты итераций уточняли изоляцию кадров, отсутствие дымчатого фона и использование равномерного chroma key. Неудачные варианты не используются игрой.

## Companion and checkpoint — 1.1.0
Generated with imagegen: beige gothic kitten, amber eyes, burgundy ribbon, three side-view poses (sitting and two walking frames), plus rooted charcoal dead-tree checkpoint with a burgundy lantern and mushrooms. Crisp pixel art matching the haunted forest. Requested isolated sprites with no text. Follow-up edit removed the dark background and external glow; the returned RGBA atlas has transparent background. Selected result: companion-original.png. Frames are extracted with point filtering by AliceArtImport.

The jump effect BranchCrunch.wav is original deterministic synthesis from Tools/make_branch_crunch.py: short irregular broadband twig cracks with a muted wooden body, without the former rising tone.

## UI and companion animation — 1.1.1

Built-in imagegen edit using `companion-original.png` as the visual reference: one isolated beige kitten RA in a clear airborne side-view jump, front paws forward, rear paws tucked, lifted tail, amber eye and burgundy ribbon; preserve the existing chunky pixel proportions and palette; transparent background; no text, ground, shadow, glow or extra objects. Selected result: `cat-jump-original.png`, imported as `cat_jump.png`.

Built-in imagegen generation: two isolated gothic life emblems in one horizontal atlas on transparency. Full state uses a dusty-rose/burgundy gem heart in black wrought-iron thorn filigree; empty state repeats the frame with a hollow cracked dark center. Crisp pixel art, readable at HUD size, no emoji shape, text or extra objects. Selected result: `life-icons-original.png`, imported as `life_full.png` and `life_empty.png`.


## Мрачный замок и погоня — 1.2.0

Все три итоговых атласа созданы встроенным `imagegen` (не CLI) на настоящем прозрачном RGBA-фоне и сохранены в `Assets/ArtSource`.

- `castle-kit-original.png`: horizontal three-cell transparent atlas; two detailed charcoal gothic masonry tiles with cracks, chipped stone, burgundy moss and roots; one tall ruined pointed castle arch with broken battlements and black vines; crisp hand-painted pixel art, no text, guides, ground strip or simple placeholders. Импортёр берёт бесшовную внутреннюю кладку для `castle_stone`/`castle_moss` и отдельную арку `castle_arch`.
- `gym-demon-original.png`: horizontal four-frame run-cycle atlas of the same muscular horned gothic bodybuilder demon, side view facing right; charcoal skin, oxblood accents, black wraps and heavy boots; consistent scale and baseline; crisp pixel art, transparent background, no text, shadow, dividers or guide marks.
- `plants-extra-original.png`: horizontal five-cell transparent atlas: black fern with burgundy tips, pale toxic mushrooms, thorny oxblood roses, wilted bell flowers and a dense marsh plant with roots and mud; organic detailed pixel silhouettes, common baseline, no horizontal red stripe, guide line, connected ground strip or geometric marks.


## Значок Windows — 1.2.1

`AliceHeart.ico` не генерировался отдельно: это точное готическое полное сердце `life_full.png` из версии 1.1.1, преобразованное без сглаживания в многоразмерный ICO (16, 24, 32, 48, 64, 128 и 256 px). Значок встроен в Windows-сборку и назначен ярлыку. Новых вызовов imagegen для версии 1.2.1 не выполнялось.

## Непрозрачность и музыка — 1.2.2

Новых вызовов imagegen для версии 1.2.2 не выполнялось. Импортёр сохраняет полностью прозрачный фон, а у всех видимых пикселей замка, Демона качалки и дополнительных растений устанавливает полную непрозрачность. Готическая тема леса и тревожная тема погони — две оригинальные процедурные композиции, созданные `Tools/compose_gothic_music.py`.

## Подземелье и «Заживление татуировок» — 1.3.0

Оба атласа созданы встроенным `imagegen` на прозрачном RGBA-фоне. Импортёр делает видимые пиксели полностью непрозрачными и сохраняет Point-фильтрацию.

- `tattoo-healing-original.png`: горизонтальный атлас из пяти ячеек. Первые четыре — отдельные позы монстра «Заживление татуировок»: рёв, рывок, атака когтями и получение удара. Образ состоит из угольных чернил, завитков татуировки, светлых растрескавшихся участков, бордовых швов и повязок; без крови, текста, рамок и фона. Пятая ячейка — готическое блюдо с выпечкой, овощами, травами и золотистым паром.
- `dungeon-kit-original.png`: горизонтальный атлас из четырёх ячеек — детализированная пещерная кладка с корнями и бордовыми прожилками, арка со спуском, арка с подъёмом и декорация со сталактитами, грибами, цепью, корнями и фонарём. Без текста, персонажей, направляющих линий и простых геометрических заглушек.

Рёв `TattooRoar.wav` — оригинальный процедурно синтезированный звук из `Tools/make_tattoo_roar.py`.

## Ландшафт и анимация подземелья — 1.4.1

Встроенный `imagegen` использован для двух новых ассетов.

- `dungeon-backdrop-v2.png`: широкая оригинальная пещера без двери или повторяющейся плитки — природные уступы, влажная каменная порода, редкие корневые кластеры, сталактиты, грибы и вросшие руины в угольно-фиолетовой и бордовой гамме. Импортируется как единый параллакс-фон `dungeon_backdrop.png`.
- `tattoo-healing-v2-original.png`: шесть изолированных поз монстра на чёрном фоне, который при импорте становится прозрачным: ожидание, два шага, подготовка, прыжок, реакция на удар и падение. Рамок, текста и теней нет.

`MonsterDefeat.wav` создан процедурно в `Tools/make_monster_defeat.py`: короткий хриплый голосовой спад с шумовой фактурой. Он используется всеми убитыми врагами вместо положительного перезвона.

## Чумной доктор и значок — 1.5.0

Встроенный imagegen, итоговый файл `doctor-original.png`. Промпт: Game asset sheet, gothic detailed pixel art. Four equal-width horizontal cells. First three: same full-body plague doctor facing right, black burgundy coat, ivory beak mask, broad black hat, cane, leather satchel; idle poses relaxed with cane, adjusting beak, settling back breathing, consistent scale and baseline. Fourth: round tarnished-silver badge, burgundy enamel and abstract ivory labyrinth. Isolated subjects, margins, no text, no labels, no shadows. Результат с прозрачным фоном сохранён в проект; импортируются три кадра doctor0–2 и doctor_badge с Point-фильтрацией.

## Демон тревожности — 1.6.0

Встроенный imagegen: новый четырёхкадровый горизонтальный атлас, цельный рогатый демон с костяной маской, угольным телом и бордовым плащом позади тела; четыре позы ходьбы вправо, единый масштаб, без текста. Уточнение к полученному атласу: “Edit this exact four-frame demon sheet. Preserve four poses, the character design, positions and proportions. Replace ALL background and glow with completely uniform flat saturated #00FF00 chroma green. Remove every shadow, smoky area and halo outside the bodies. Clean cutouts with sharp silhouette edges. Keep opaque character artwork unchanged. No green within character. Output four evenly spaced cells.”

Итог: `anxiety-v2-original.png`. Импортёр удаляет зелёный фон, обрезает ячейки и создаёт `Resources/Art/demon0–3.png` высотой до 156 пикселей, с Point-фильтрацией и непрозрачным силуэтом. Разворот влево выполняется зеркально в игре.

## Финальные анимации и предметы — 1.6.0

Встроенный imagegen. Итоговые файлы в Assets/ArtSource: gym-demon-v2-original.png (12 кадров), tattoo-healing-v3-original.png (12 кадров), doctor-v2-original.png (8 кадров), feast-v2-original.png, wine-v2-original.png. Импортёр выделяет основной связный силуэт каждой ячейки и убирает отдельные чужие фрагменты. Фон удаляется перед нарезкой.

### gym

Create a new production pixel-art animation sprite sheet using the reference only for the identity of the muscular charcoal and burgundy horned gym demon. STRICT layout: FOUR columns THREE rows = twelve equal cells. All full bodies face RIGHT in profile, same scale, fixed bottom foot baseline, 12 percent empty margin in every cell, no body or clothing crosses any cell. Row one: four consecutive run cycle poses contact-down-passing-up. Row two: four consecutive opposite-leg run poses contact-down-passing-up, visibly alternating legs and counter-swinging arms. Row three: breathing idle inhale, breathing idle exhale, airborne tucked jump, landing crouch. Detailed gothic game pixel art, sharp pixel clusters, no motion blur. Each character a single connected clean silhouette, no loose scraps or fragments. Background pure uniform saturated chroma green #00FF00, no shadows, no glow, no ground, no text, no lines. Preserve monstrous broad body, curved black horns, burgundy wraps, massive fists and boots, no weapons.

### tattoo

Create a production animation sheet replacing the reference sheet. Reference identity: hunched pale ivory tattoo-healing monster, charcoal tattoo swirls, burgundy wraps, skull mask, long black claw hands. Gothic detailed PIXEL ART. STRICT FOUR columns by THREE rows, twelve equal cells, no cell borders or labels. All monsters full body in RIGHT facing profile. Each fits ENTIRELY within its cell with 12 percent clear margin. Same body scale and baseline. Row one four consecutive walking poses: contact, down, passing, up with visibly changing legs and opposite arm swings. Row two opposite-leg contact, down, passing, up completing a full eight-frame walking cycle. Row three: head lifted mouth wide open ROAR, arm reaching forward ATTACK with bent knees, airborne tucked JUMP, collapsed on ground DEATH. Clean opaque continuous silhouettes, no stray disconnected fragments, no particles or loose strips. Uniform pure #00FF00 saturated green background, no shadows, no floor, no glow, no text. Make all twelve cells separate and consistent. Preserve grotesque but non-gory dark fantasy character design.

### doctor

Recreate the plague doctor from the reference as an EIGHT frame smooth idle animation sprite sheet. Omit badge. Strict FOUR columns TWO rows, equal cells. Every full body faces RIGHT, consistent character scale and feet baseline, 15 percent empty padding inside every cell, NEVER cross a cell boundary. Gothic detailed pixel art, ivory beak mask, black hat and coat, burgundy cape, brass ornaments, cane touching ground, visible boots. Eight consecutive poses forming a gentle loop: relaxed exhale, inhaling chest raised slightly, head turns subtly, right hand begins rising, hand touches mask beak, hand lowers halfway, hand returns to cane, settles back to relaxed. Keep boots exactly same position, cane connected to hand, no levitation, no loose particles or stray bits. Clean opaque sharp silhouettes on completely uniform pure saturated chroma GREEN #00FF00 background. No black background, no gradient, no halo, no shadows, no text or framing. Entire figure fits each cell.

### food

Single isolated item for a gothic pixel-art game's reward UI: a perfectly centered round dark pewter platter of delicious golden roast chicken, potatoes, a few red vegetables and herbs. Three-quarter front view, horizontal platter, symmetric balanced visual mass around canvas center. Detailed crisp pixel clusters, limited warm gold burgundy brown palette. Entire object fits inside central 70 percent of square canvas, generous equal empty margins. NO steam, no floating particles, no detached fragments, no shadows, no light circle, no text, no utensils, no scenery. Pure flat uniform saturated chroma green #00FF00 background, no green pixels in food. Clean connected silhouette. It must read clearly at 96 pixels.

### wine

Single isolated inventory pickup sprite for a gothic pixel-art platform game: an upright burgundy glass wine elixir bottle with a cork, ivory oval label bearing a small abstract gold crescent, no text. Full bottle perfectly vertical and centered, symmetrical silhouette, sharp clean edges, detailed crisp pixel clusters. Entire bottle occupies center 70 percent of square canvas height and 30 percent of width, generous empty margins. Deep burgundy liquid, dark charcoal glass outlines, gold highlights, cream label, no green inside bottle. Pure flat uniform saturated chroma green #00FF00 background. No floor, shadow, glow, particles, other objects, stray fragments or lettering. Readable at 48 pixels tall.

Для листов tattoo и doctor дополнительная правка: preserve all character poses and positions; replace ALL background and halos with perfectly flat saturated #00FF00; no shadows, gradients, dark background or green inside characters. Итоговые позы сохранены, фон удалён при импорте.

Заставка splash.png предоставлена пользователем 15.10.2026 и используется без генерации или изменения изображения в начале игры и HTML-загрузчике.
