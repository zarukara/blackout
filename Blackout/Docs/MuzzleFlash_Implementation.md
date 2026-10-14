# Player Muzzle Flash

14.10.2026. Исходный HEAD: `3a5db36`, рабочее дерево было чистым.

## Реализация

Переиспользованы существующие `Assets/_Presentation/Sprites/Muzzle Flash.png`, `VFX/MuzzleFlash.prefab` и `VFX/M_MuzzleFlash.mat`. Их GUID сохранены. PNG не изменён: 1024×1024, сетка 2×2, прозрачный фон и полупрозрачные края. Mip Maps отключены, чтобы соседние варианты не смешивались на малом размере; остальной подходящий импорт сохранён.

`WeaponSystem.MuzzleFlashVfx` воспроизводит заранее созданный Particle System. `PistolWeapon` вызывает его непосредственно после `Projectile.Launch`; добавлена проверка полученного снаряда на null. Урон, скорость, spread, cooldown, прицеливание и ввод сохранены.

Параметры prefab: Lifetime **0,06 с**, Size **0,45**, Speed **0**, Local simulation, Shape scaling, Max Particles **4**, Loop/Play On Awake выключены. Emission: нулевая частота, один Burst из одной частицы для обычного проигрывания prefab. В игре компонент вызывает **Emit(1)** без Play: каждый вызов создаёт одну частицу, не перезапускает прошлую вспышку и не добавляет второй Burst. Переключение оружия само по себе эффект не запускает; выключение VFX очищает оставшиеся частицы.

Texture Sheet Animation: Grid 2×2, Whole Sheet, Cycles 1, **Frame over Time = Constant 0**, Start Frame = Random Between Two Constants. В API Unity 6000.3.8f1 используются нормализованные границы **0–1**. Проверка BakeMesh/UV подтвердила четыре интервала четвертей диапазона; значения выше 1 зажимаются последним кадром. Случайность берётся из Particle System, без дополнительного расходования `UnityEngine.Random` игровой стрельбы. Кадр частицы остаётся неизменным.

Материал: **Universal Render Pipeline/Particles/Unlit**, Transparent/Additive, SrcAlpha → One, ZWrite Off, Flipbook Blending Off. Исходный atlas назначен в Base Map и Emission Map. Emission включён; HDR RGB **(8; 7,5; 6,5)** даёт слегка тёплое белое свечение. Фон отсекается исходной альфой, прямоугольной подложки нет.

Renderer: Billboard/View. Компонент поворачивает изображение по проекции направления дула на камеру, не меняя игровой Transform или направление выстрела. Pivot X **0,44** ставит начало изображения у ствола. Shape scaling сохраняет размер вспышки при существующем масштабе модели оружия ×17.

## Подключение

Иерархия: `Player/View/…/mixamorig:RightHand/Glock-19/ShootPoint/MuzzleFlash`. Локальные Position/Rotation — нулевые, Scale — единичный. Существующий VFX был под выключенным дублем Glock; в Player prefab он перенесён к действительному `PistolWeapon.firePoint`. В Level_01 учтён отдельный override ShootPoint; назначен экземпляр того же prefab. Оружие и точки выстрела не перемещались.

В Main Camera включён Post Processing. HDR уже был включён у камеры и активного URP Asset. Добавлен сценовый `Muzzle Flash Bloom` (Global Volume) с отдельным профилем `VFX/MuzzleFlashBloom.asset`: Intensity **0,35**, Threshold **1,2**, Scatter **0,6**, High Quality Filtering включён. DefaultVolumeProfile, экспозиция, тонмаппинг, Minimap Camera и файлы `_Support` не изменены. Новых источников света нет.

## Проверка

Unity **6000.3.8f1**, batchmode с графикой, изолированная копия текущего проекта. Unity Assets настроены и сохранены через Editor API; в основной проект перенесены только перечисленные файлы. Их содержимое сверено с проверенной копией. Временные Editor-скрипты и копия проекта удалены после работы.

- Компиляция успешна; финальный сценарный PlayMode-runner завершился с **0 runtime errors**. Это не NUnit Test Runner.
- 256 эмиссий: распределение по четырём UV-ячейкам **64 / 72 / 55 / 65**. UV совпали на возрасте 0 / 0,02 / 0,04 с. Реальное течение времени удаляет частицу после 0,06 с.
- Проверены успешный выстрел, отклонение по cooldown, отсутствие пула/дула и искусственно вызванный отказ выдачи снаряда. При отказах вспышек нет. Поведение отказа самого пула не изменялось.
- Четыре успешных выстрела подряд дают четыре частицы; повторная стрельба не создаёт новые VFX-объекты или материалы. Проверены переключения оружия и очистка при выключении.
- Запущен Level_01: стрельба в четырёх направлениях и при реальном движении игрока. Вспышка остаётся на ShootPoint. В тесте пистолет получен существующим `PickupWeapon`; стартовое оружие проекта не менялось.
- Осмотрены GPU-кадры сцены и отдельный крупный план четырёх вариантов. Сравнение одной вспышки с Bloom и без него: **810 пикселей ореола вне яркого ядра**, максимальная разница канала около **0,451**. Это подтверждение отрисованного эффекта, а не только настроек Inspector.

[Результаты](../Logs/Muzzle-tests.txt), [проверка исходной текстуры и Start Frame](../Logs/Muzzle-probe.txt), [кадры PlayMode](../Logs/MuzzleVisualReview/), [git diff --stat](../Logs/Muzzle-diff-stat.txt). Исходники временных диагностических скриптов сохранены в `Logs/MuzzleValidationSources/` для воспроизведения.

## Файлы и ручные действия

Созданы:
- `Assets/_Source/WeaponSystem/MuzzleFlashVfx.cs` и `.meta`.
- `Assets/_Presentation/VFX/MuzzleFlashBloom.asset` и `.meta`.
- Этот отчёт.

Изменены:
- `Assets/_Source/WeaponSystem/PistolWeapon.cs`.
- `Assets/_Presentation/Prefabs/Player.prefab`.
- `Assets/_Presentation/Scenes/Level_01.unity`.
- `Assets/_Presentation/Sprites/Muzzle Flash.png.meta`.
- `Assets/_Presentation/VFX/M_MuzzleFlash.mat`.
- `Assets/_Presentation/VFX/MuzzleFlash.prefab`.
- `Docs/VerticalSlice_ImplementationStatus.md`.

Назначения Inspector уже сохранены. Достаточно дождаться импорта Unity и открыть Level_01; если сцена была открыта до внешних изменений, перезагрузить её после выхода из PlayMode. Получение и выбор пистолета выполняются прежним игровым способом.

Ограничения: standalone build и другие графические API не проверялись; полный регресс боёв и анимаций не проводился. Bloom действует на яркие HDR-пиксели камеры, поэтому не является избирательным эффектом только для одного объекта. Ranged Police VFX, звук, отдача, гильзы и остальные эффекты в эту задачу не входят. Commit/reset/discard не выполнялись.
