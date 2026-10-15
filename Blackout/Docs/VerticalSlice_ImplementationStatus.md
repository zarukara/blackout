# BlackHost: combat / animation integration

## 15.10.2026 — Enemy Death Gore / Blood Explosion / Blood Pools

P0 и P1 реализованы и подключены в Level_01 к `Enemy.prefab` и `RangeEnemy.prefab`. Основа — HEAD `3193499`, рабочее дерево было чистым. `EnemyDeathVfx` подписан на существующий Health.Died, однократно скрывает закэшированные Renderers модели/оружия и очищает локальный Muzzle Flash. Корневой Enemy не отключается компонентом VFX: прежние Dead FSM, destroyOnDeath, события спавнера, Grab execution/heal и отдельный оружейный loot сохранены. Независимые мировые эффекты не скрываются вместе с моделью. Летальный Damaged пропускает обычный Hit Blood только при готовой системе Death VFX; несмертельные попадания остаются прежними.

DeathBloodBurst: два крупных atlas-всплеска 3–3,8 м, lifetime 0,18–0,24 с; 24–30 радиальных капель с гравитацией, lifetime 0,35–0,8 с. Переиспользованы материалы/текстуры Hit Blood без изменения его prefab; Alpha Blend, без emission. Шесть физических GoreFragment на смерть, три процедурных низкополигональных Mesh/material/prefab-варианта; импульс наружу и вверх, вращение, столкновения только с Ground/Wall. Через 3,5–5 с фрагменты уменьшаются и возвращаются в пул. Их слой Ignore Raycast исключён из игровых попаданий; Collider layer overrides исключают игрока, врагов и другие фрагменты.

BloodPool: четыре заранее созданные формы с 32 сегментами неправильного контура, случайные форма/поворот/пропорции, процедурная текстура 128×128 с вариациями тона. Материал URP Unlit, бордовый, без emission/Bloom; влажный PBR-блеск не используется ради читаемости в текущем освещении. Размер растёт с 0,3 до 2,5–4 за 1 с по cubic ease-out после задержки 0,1 с. Raycast до 50 м по Ground, ориентация по нормали и offset 0,015–0,023 м; без подходящей земли пятно не создаётся. Пятна сохраняются до вытеснения самого старого либо выгрузки/отключения пула.

Ограничения: 12 заранее созданных burst instances, максимум 384 частицы Death Blood и 72 физических фрагмента, 40 постоянных луж. При насыщении используются те же экземпляры. Новые GameObject, Mesh и Material для Death VFX на каждой смерти не создаются; существующий EnemyWeaponDrop продолжает создавать отдельный игровой pickup.

Новый C#: `EnemySystem/Combat/EnemyDeathVfx.cs`, `VfxSystem/DeathBloodBurst.cs`, `DeathVfxPool.cs`, `BloodPoolVfx.cs`, `GoreFragment.cs` — всё под `Assets/_Source/`, с .meta. Изменены `EnemyHitVfx.cs`, `EnemyFacade.cs`, `EnemySpawnContext.cs`, `ArenaWaveSpawner.cs`, `Bootstrapper.cs`: только выбор lethal VFX и передача зависимости. Health, оружие, Projectile, AI и Grab/Throw не изменены.

Assets через Unity Editor API в `Assets/_Presentation/VFX/Blood/Death/`: `DeathBloodBurst.prefab`, `BloodPool.prefab`, `GoreFragment_0..2.prefab`, `BloodPoolShape_0..3.asset`, `GoreFragmentMesh_0..2.asset`, `M_BloodPool.mat`, `M_GoreFragment_0..2.mat`, `T_BloodPool.png` и .meta. В обоих Enemy prefab назначены тело/Renderers/локальные Particle Systems. В Level_01 добавлен только `Enemy Death VFX Pool` и его ссылки в Bootstrapper/спавнере; уровень, URP, Hit Blood, Muzzle Flash и Bullet Tracer не переделывались.

Unity 6000.3.8f1: P0 — 30 проверок; итоговая версия с фрагментами — 39 сценарных PlayMode-проверок, 0 runtime/editor errors. Проверены реальные пули, двойное убийство когтями обоих типов врагов, Grab execution/heal, физический воздушный Throw в стену, однократность/отсутствие lethal Hit Blood, скрытие оружия, Dead/targeting, стрельба живого ranged и её прекращение после смерти, падение фрагментов на пол, отсутствие gameplay collisions, растекание/сохранение, склон/отсутствие земли, 10 одновременных смертей, 85 дополнительных запусков для насыщения пула, отсутствие новых Mesh/Material, возврат всех фрагментов, переход ко второй волне и ArenaCompleted.

Результаты: `Logs/Death-tests.txt`, `Logs/Death-errors.txt`, `Logs/Death-P0-tests.txt`; GPU-снимки игровой камеры и крупные планы: `Logs/DeathVisualReview/`. Это графический batch PlayMode на RTX 3070 и визуальная оценка его кадров, не ручной интерактивный прогон Game View, не NUnit и не проверка standalone-билда/FPS. Временные инструменты сохранены вне Assets в `Logs/DeathValidationSources/`; в production нет Integration/Editor или тестовых объектов. Ручных назначений нет: дождаться импорта, открыть Level_01 и проверить пистолет/когти/Throw. Коммитов агент не выполнял.

## 15.10.2026 — жёлтый трассер без круглой головки

Изменены только `Bullet.prefab` и `M_BulletTrail.mat`: Base Color RGB (1; 0,78; 0), HDR Emission (4; 3; 0). Отображение круглого BulletCore отключено через ParticleSystemRenderer; Trail Renderer имеет заострённый передний край, без круглых caps, максимальную ширину 0,07 и прежний lifetime 0,08 с. C#, пул, урон, физика и глобальный Bloom не изменены. Ручных назначений нет.

Unity 6000.3.8f1: компиляция и 7 проверок графического PlayMode; итоговый прогон без runtime/editor errors. Просмотрены GPU-снимки реального выстрела, четырёх направлений, повёрнутой камеры и крупного плана; жёлтый след читается без отдельного шара на конце. Результаты: `Logs/BulletStyle-tests.txt`, `Logs/BulletStyleVisualReview/`. В первом прогоне возникло исключение внутреннего UnityEditor.Search при индексации; оно сохранено в `Logs/BulletStyle-first-run-editor-errors.txt`, в итоговом прогоне не повторилось.

## 15.10.2026 — Bullet Projectile VFX завершён

В общий `Assets/_Presentation/Prefabs/Bullet.prefab`, используемый Level_01, встроены BulletCore и BulletTrail. Одна локальная billboard-частица размером 0,14, Start Speed/Gravity = 0; Shape выключен, Scaling Mode = Shape сохраняет размер при исходном масштабе Bullet ×3. Trail: Time 0,08 с, Width 0,05 с сужением до нуля, Min Vertex Distance 0,02, Stretch, View alignment, alpha fade. Тени отключены. Старый MeshRenderer выключен; Rigidbody, Collider, lifetime, hitLayers и параметры оружия сохранены.

Код: новый `Assets/_Source/ProjectileSystem/ProjectileVfx.cs`; в `Projectile.cs` добавлены кэширование компонента, очистка до перемещения и запуск после Launch. Checkout/OnEnable оставляет эффект пустым; после установки позиции запускается одна частица на срок жизни пули и включается след. OnDisable при возврате в пул останавливает/очищает частицы и выключает/очищает TrailRenderer. ProjectilePool и оба оружия не изменены.

Созданы через Unity Editor API: `Assets/_Presentation/VFX/Bullet/T_BulletGlow.png`, `T_BulletTrail.png` (процедурные RGBA 64×64), `M_BulletGlow.mat`, `M_BulletTrail.mat` и соответствующие .meta. Материалы URP Particles/Unlit, Additive, HDR Emission RGB соответственно (4; 3,8; 3,2) и (2,5; 2,3; 1,8). Использован существующий Bloom без изменения Volume/URP/экспозиции. Все ссылки в prefab назначены.

Проверено Unity 6000.3.8f1 в изолированной копии Level_01: компиляция и 68 сценарных PlayMode-проверок, 0 runtime errors. Проверены пять запусков одного экземпляра из удалённых точек, очистка на checkout/return/active relaunch/истечении lifetime/попадании, положение следа позади пули, стабильное ядро, реальные выстрелы игрока и ranged FSM, исходный урон обеих сторон, кровь с привязкой к врагу, летальная пуля/Dead state, Weapon Wheel. Серия 30 выстрелов сохранила исходные 20 projectile instances и количество материалов 101 → 101.

Визуальная проверка выполнена по GPU-снимкам Camera.main из графического batch PlayMode (RTX 3070): четыре направления, повёрнутая камера, общий план/крупный план, игрок/полицейский, кровь после попадания. Сравнение одного кадра с Bloom/без него подтвердило свечение. Это не ручная интерактивная проверка окна Game View и не NUnit-тесты. Результаты: `Logs/Bullet-tests.txt`, `Logs/Bullet-errors.txt`; изображения: `Logs/BulletVisualReview/`; воспроизводимые временные инструменты сохранены вне Assets в `Logs/BulletValidationSources/`. Временного проекта `Temp/BulletValidation` больше нет.

Ручных назначений Inspector/Animator нет. Открыть Level_01, дождаться импорта Unity, выбрать пистолет и стрелять. Реализация уже присутствует в текущем HEAD `dc8392b` (13 файлов, +5592/−2, включая Unity-сериализацию и .meta); агент Git commit не выполнял. Исходная база задачи — `9ff3b9a`. Текущие посторонние изменения TMP и Assets/_Recovery сохранены.

## 14.10.2026 — следование Hit Blood за врагом

Основной ImpactSplash переведён в Local simulation и следует за точкой попадания относительно Chest в LateUpdate после анимации/наведения. Капли остаются в World simulation и продолжают собственные траектории. При отключении/уничтожении цели эффект заканчивается на последней позиции; при возврате в пул ссылка на цель очищается. Изменены `EnemyHitVfx.cs`, `HitBloodVfxPool.cs`, `HitBloodVfx.cs` и одна настройка в `HitBloodVfx.prefab`.

Unity 6000.3.8f1: компиляция и 15 проверок в графическом PlayMode — перемещение/поворот/движение Chest, положение самой частицы, независимость капель, две цели, отключение/уничтожение цели, возврат и повторное использование пула; 0 runtime errors. Результаты: `Logs/BloodFollow-tests.txt`, снимки `Logs/HitBloodVisualReview/blood-follow-before.png` и `blood-follow-after.png`. Ручных назначений нет; существующий урон и параметры частиц сохранены.

## 14.10.2026 — Hit Blood VFX

Основа: HEAD `a316c25`. Исходный `Assets/_Presentation/VFX/BlackHost_RedLiquid_Hit_2x2.png` найден: RGBA 1024×1024, 947898 полностью прозрачных пикселей, четыре ячейки с прозрачными отступами. PNG не изменён; импорт настроен на alpha transparency, Clamp, Bilinear, без mipmaps и компрессии. AGENTS.md и прежнего VerticalSlice_Functionality.md в текущем checkout нет; использовано приложенное ТЗ.

Готовый `HitBloodVfx.prefab`: ImpactSplash — одна billboard-частица 0,16 с, native random Start Frame 0..1 для сетки 2×2, Frame over Time = 0 (кадр не меняется), размер 0,92–1,12 с небольшим ростом и alpha fade. Droplets — 5–10 отдельных овальных капель, размер 0,035–0,08, скорость 1,4–3,4, lifetime 0,28–0,58 с, конус 28°, гравитация 1,1, уменьшение и затухание. Оба материала URP Particles/Unlit, обычный Alpha Blend, без Emission/Additive; существующий Bloom не изменён.

`Health.Damaged(DamageInfo)` вызывается после подтверждённого урона, содержит фактический урон, контакт, направление и IsLethal. EnemyHitVfx подписан только на здоровье врагов. Пули передают ближайшую точку коллайдера, когти — ближайшую точку на уровне корпуса, Throw — ContactPoint; старый TakeDamage(int) поддержан с fallback на Chest. Летальный удар сейчас даёт один обычный Hit Blood; IsLethal позволяет позже заменить его отдельной веткой Death VFX. Существующие HealthChanged/Died, урон, cooldown, попадания и FSM сохранены.

В Level_01 назначен общий пул: 32 заранее созданных экземпляра, поиск свободного слота, при насыщении переиспользование по кольцу. Новых Instantiate/Destroy и материалов на попаданиях нет. Particle Systems очищаются перед повторным использованием и возвращаются после завершения. World simulation капель и отдельный сценовый родитель позволяют каплям пережить уничтожение врага. Пул передаётся спавнером через EnemySpawnContext/EnemyFacade; сценовые враги получают его один раз из Bootstrapper.Start.

Изменённый код:

- `Assets/_Source/CombatSystem/Health.cs`
- `Assets/_Source/ProjectileSystem/Projectile.cs`
- `Assets/_Source/WeaponSystem/ClawsWeapon.cs`
- `Assets/_Source/EnemySystem/Grab/ThrownEnemyDamage.cs`
- `Assets/_Source/EnemySystem/Core/EnemyFacade.cs`
- `Assets/_Source/EnemySystem/Core/EnemySpawnContext.cs`
- `Assets/_Source/ArenaSystem/ArenaWaveSpawner.cs`
- `Assets/_Source/CoreSystem/Bootstrapper.cs`

Новый код с .meta: `Assets/_Source/CombatSystem/DamageInfo.cs`, `Assets/_Source/EnemySystem/Combat/EnemyHitVfx.cs`, `Assets/_Source/VfxSystem/HitBloodVfx.cs`, `Assets/_Source/VfxSystem/HitBloodVfxPool.cs`; также .meta новой папки VfxSystem.

Assets, сохранённые через Unity Editor API: изменены `Assets/_Presentation/Prefabs/Enemy.prefab`, `Assets/_Presentation/Prefabs/RangeEnemy.prefab`, `Assets/_Presentation/Scenes/Level_01.unity`, importer исходного атласа. В `Assets/_Presentation/VFX/Blood/Hit/` созданы `HitBloodVfx.prefab`, `M_BloodImpact.mat`, `M_BloodDroplets.mat`, процедурная текстура `BloodDroplet.png` 64×64 и соответствующие .meta файлов/папок. Другие assets и пользовательские изменения TMP сохранены.

Проверено Unity 6000.3.8f1: компиляция и сценарный PlayMode с графикой в изолированной копии, реальные столкновения пуль, промах/стена, повторные попадания, multi-target/multi-collider когти, legacy damage, native random atlas/неизменные UV, lifetime/гравитация, 10 врагов, 96 одновременных попаданий, предел 32 эффекта/352 частицы, возврат/повторное использование пула, отсутствие новых GameObject/Material и managed allocation в цикле воспроизведения. Проверены Grab execution/heal, Throw в стену/врага, урон 75/100, смерть/единственный Died, жизнь VFX после уничтожения врага, счётчик спавнера, ranged FSM, общий ProjectilePool и оба Muzzle Flash. Runtime errors: 0. Это сценарные проверки, не NUnit; замеры времени batch Editor не являются гарантией FPS игрового билда.

Подробные результаты и замеры: `Logs/HitBlood-tests.txt`, `Logs/HitBlood-errors.txt`; снимки `Logs/HitBloodVisualReview/`, исходники проверок `Logs/HitBloodValidationSources/`, `git diff --stat` и список untracked — `Logs/HitBlood-git-diff-stat.txt`, `Logs/HitBlood-new-files.txt`. В основной Assets тестовые инструменты не добавлялись. Ручных назначений Inspector не требуется: открыть обновлённую Level_01 после импорта и запустить PlayMode. Death Burst, Decals, Gore, новые анимации и оружие не добавлялись; commit/reset/discard не выполнялись.

## 14.10.2026 — Muzzle Flash стреляющего полицейского

В `Assets/_Source/EnemySystem/Combat/EnemyRangedAttack.cs` добавлены ссылка на существующий `MuzzleFlashVfx`, проверка результата checkout и вызов `Play()` сразу после `Projectile.Launch` в LateUpdate. Через Unity Editor API сохранён `Assets/_Presentation/Prefabs/RangeEnemy.prefab`: вложенный исходный `Assets/_Presentation/VFX/MuzzleFlash.prefab` находится в `View/mixamorig:Hips/.../mixamorig:RightHand/Glock-19 (1)/ShootPoint/MuzzleFlash`, local position/rotation = 0, scale = 1. ShootPoint, характеристики оружия, FSM, анимации и Player VFX не изменены. Общие материал, атлас и Bloom переиспользованы без изменений.

Цепочка ArenaWaveSpawner → EnemySpawnContext → EnemyFacade → EnemyRangedAttack передаёт общий сценовый пул корректно; исправления пула не потребовались. В Level_01 нет вручную размещённых RangeEnemy. Отдельно проверен экземпляр с назначенными существующими сериализуемыми ссылками на пул и цель.

Unity 6000.3.8f1: компиляция и графический сценарный PlayMode в изолированной копии текущего проекта, 0 runtime errors. Реальная навигация → остановка → стрельба; четыре врага сделали 15/8/8/8 автономных выстрелов, каждый с одной собственной вспышкой. Проверены урон, lifetime, 128 эмиссий со всеми четырьмя неизменными UV-кадрами (29/36/26/37), cooldown/нет цели/нет пула/ошибка checkout, Grab/Throw/recovery/смерть, отсутствие новых GameObject/ParticleSystem/Material при повторной стрельбе после прогрева пула, пистолет игрока и Weapon Wheel. GPU-сравнение Bloom: 3941 пиксель ореола вне яркого ядра. Это сценарные проверки, не NUnit и не полный регресс всей игры; melee проверен на отсутствие изменений prefab/кода.

Результаты: `Logs/EnemyMuzzle-tests.txt`, `Logs/EnemyMuzzle-errors.txt`; снимки: `Logs/EnemyMuzzleVisualReview/`. Код проверок сохранён в `Logs/EnemyMuzzleValidationSources/`, в Assets тестовые инструменты не добавлены. Ручных назначений Inspector/Animator не требуется: дождаться импорта Unity и запустить PlayMode. Изменены только два указанных файла и этот статус; прежние незакоммиченные изменения сохранены. Git commit не выполнялся.

## 14.10.2026 — Muzzle Flash пистолета игрока

Поверх чистого HEAD `3a5db36` настроены существующие atlas/material/Particle System prefab, добавлен `WeaponSystem.MuzzleFlashVfx`, вызов после успешного Projectile.Launch и ссылки в Player prefab/Level_01. Одна частица на выстрел, один случайный неизменный кадр 2×2, Lifetime 0,06 с; HDR Emission и отдельный сценовый Bloom. GUID существующих assets сохранены, механика стрельбы и анимации не изменялись.

Unity 6000.3.8f1: компиляция и сценарный PlayMode с графикой, 0 runtime errors. Проверены 256 эмиссий/все четыре кадра, стабильность UV, cooldown/отказы, повторные выстрелы, четыре направления, движение и реальная отрисовка Bloom. Полный отчёт, перечень файлов и ограничения — [MuzzleFlash_Implementation.md](MuzzleFlash_Implementation.md). Новых ручных назначений Inspector нет; после импорта перезагрузить ранее открытую сцену.

## 13.10.2026 — архитектурный рефакторинг

Поверх чистого HEAD `be6edc6` просмотрены 57 собственных скриптов. Вынесены общий расчёт движения относительно камеры, данные/подготовка очереди волн и подключение компонентов игрока в PlayerFacade. Изменены шесть существующих C#-файлов, добавлены четыре типа с `.meta`; полный перечень и анализ — в [ArchitectureReview.md](ArchitectureReview.md).

Unity 6000.3.8f1: обе версии скомпилированы и сравнены во временной копии. Совпали 176 записей сериализуемых полей, 320 вариантов очереди волн/Random, 28 вариантов движения/Dash-направления/параметров Animator, подписки и UI-переходы; проверен запуск Level_01. Runtime errors: 0 до и после. Подробности: [ArchitectureValidation.txt](../Logs/ArchitectureValidation.txt). Проверка сценарная, не NUnit и не полный регресс всех игровых механик. Игровые assets не редактировались, ручных изменений Inspector не требуется.

Ниже сохранён исторический отчёт от 12.10; его результаты не являются проверкой текущего HEAD.

Дата: 12.10.2026. Основа: HEAD `3418afc`, рабочее дерево при начале задачи было чистым.
В этом HEAD отсутствовали корневой AGENTS.md, прежнее ТЗ VerticalSlice_Functionality.md и ранее обсуждавшиеся coordinator/windup-системы. Работа выполнена поверх фактической FSM, без восстановления старых копий.

## Выполнено

- **Ranged Run:** в RangeEnemy отсутствовал EnemyAnimationController. Подключён существующий общий компонент, проверены Speed(float), Avatar, Idle/Run и выключенный root motion. Темп Run следует реальной скорости агента; устранён повторный Warp при обычной смене состояний.
- **Pool / стрельба:** Bootstrapper передаёт общий сценовый пул игроку и размещённым врагам; спавнер продолжает передавать EnemySpawnContext. Исправлена передача цели до/после Awake. Прицеливание использует высоту цели, выстрел происходит после Animator и разворота root. ShootPoint привязан к стволу; aimPivot поворачивает верх корпуса вместе с обеими руками, сохраняя хват оружия.
- **Melee:** Bake Into Pose для Root Transform Rotation выключен у Attack и Run. В исходном Attack корпус отклонялся от root более чем на 90°. Дубинка перенесена с RightHandIndex1 на RightHand, исправлен хват. Урон возникает через 0,4 с, цикл анимации — 1 с; разворот завершается перед атакой, направление фиксируется на время удара. Grab/Death отменяют атаку. Damage 10, distance 1,7 и cooldown 1 сохранены.
- **Игрок:** поворот и оружие используют один targeting и InputReader; снято обнуление высоты выстрела. Анимация когтей запускается только при принятой атаке, исправлено запечённое вращение ClawsAttack. Переключение верхних слоёв сглажено за 0,12 с. Движение анимируется по фактической скорости CharacterController. Темп направленных клипов откалиброван для существующих 6 м/с; короткий правый шаг заменён зеркальным RunLeft. Исходные FBX не переписаны.
- **Prefab:** у Player установлен слой Player (3), учитываемый пулей; у RangeEnemy исправлен центр CapsuleCollider и отключён лишний CharacterController. Баланс скорости перемещения/урона/дальности/кулдаунов оружия сохранён.
- **Outline RangeEnemy:** ссылка на Renderer исходного FBX заменена ссылкой на SkinnedMeshRenderer самого prefab. Используется прежний M_EnemyOutline — тот же материал и shader, что у MeleeEnemy; shader не изменён.
- По просьбе пользователя **Assets/_Source/Integration, Integration/Editor и их содержимое удалены**. Диагностика выполнялась во временной копии, в игровые Assets тестовые компоненты не включены.

## Проверка

Unity 6000.3.8f1, изолированная копия проекта, batchmode **с графикой**, реальный PlayMode / физика / NavMesh / Animator / Cinemachine. Компиляция успешна. Финальный прогон: **11/11**, runtime errors **0**. Это временный сценарный PlayMode-runner, не NUnit Test Runner.

Проверены: полный цикл ranged Chase/Run → Idle/выстрел → Chase; попадания с четырёх сторон; Grab/Throw стрелка; выбранная цель при четырёх ракурсах камеры и свободная 3D-траектория игрока; четыре направления движения; melee с четырёх сторон и три врага одновременно; отмена атаки; claws cooldown/execution/heal; Dash; инициализация Level_01 и общий pool для ручного/спавненного стрелка; Cinemachine orbit; weapon wheel, pause/resume/death; hover/clear outline на обоих врагах. Свободное прицеливание отдельно повторено 8 раз успешно.

Осмотрены кадры реального PlayMode: Run, прицеливание, несколько фаз Attack/ClawsAttack, хват дубинки, outline. Измеренная скорость опорной стопы у направленных анимаций игрока после настройки — 5,996–6,001 м/с. Снимки подтверждают проверенные позы; субъективная «идеальность» всей анимации ими не доказывается.

Локальные результаты: [PlayMode report](../Logs/CombatIntegrationTests.txt), [измерения движения](../Logs/CombatMotionAudit.txt), [кадры проверки](../Logs/CombatVisualReview/).

## Unity Inspector / ручные действия

Выйти из старого PlayMode, дождаться импорта изменённых assets и снова запустить `Assets/_Presentation/Scenes/Level_01.unity`. Новые ссылки, компоненты и настройки уже сохранены; вручную назначать их не требуется. Спавнер волн оставлен в исходном выключенном состоянии сцены; тест включал его только в PlayMode. Проверки не являются релизным тестом standalone build.

## Изменённые файлы

Пути ниже относительно корня Unity-проекта. Для ревью: `git diff --stat` и `git diff` (новый CombatAim.cs также просмотреть отдельно). Commit/reset/discard не выполнялись.

```text
Assets/_Presentation/Animations/Enemies/MeleePolice/PoliceMelee Animator.controller
Assets/_Presentation/Animations/Enemies/MeleePolice/PoliceMelee Attack.fbx.meta
Assets/_Presentation/Animations/Enemies/MeleePolice/PoliceMelee Run.fbx.meta
Assets/_Presentation/Animations/Enemies/RangePolice/RangePolice Animator.controller
Assets/_Presentation/Animations/Player/ClawsAttack.fbx.meta
Assets/_Presentation/Animations/Player/Player Animator.controller
Assets/_Presentation/Prefabs/Enemy.prefab
Assets/_Presentation/Prefabs/RangeEnemy.prefab
Assets/_Presentation/Prefabs/Player.prefab
Assets/_Presentation/Scenes/Level_01.unity
Assets/_Source/CombatSystem/CombatAim.cs (+ .meta, новый)
Assets/_Source/CoreSystem/Bootstrapper.cs
Assets/_Source/EnemySystem/Combat/EnemyMeleeAttack.cs
Assets/_Source/EnemySystem/Combat/EnemyRangedAttack.cs
Assets/_Source/EnemySystem/Core/AEnemy.cs
Assets/_Source/EnemySystem/Core/EnemyAnimationController.cs
Assets/_Source/EnemySystem/Core/EnemyContext.cs
Assets/_Source/EnemySystem/Core/EnemyFacade.cs
Assets/_Source/EnemySystem/Movement/EnemyMovement.cs
Assets/_Source/EnemySystem/States/EnemyChaseState.cs
Assets/_Source/EnemySystem/States/EnemyMeleeAttackState.cs
Assets/_Source/PlayerSystem/PlayerAnimationController.cs
Assets/_Source/PlayerSystem/PlayerInputReader.cs
Assets/_Source/PlayerSystem/PlayerMovement.cs
Assets/_Source/PlayerSystem/PlayerRotation.cs
Assets/_Source/PlayerSystem/PlayerTargetingController.cs
Assets/_Source/ProjectileSystem/Projectile.cs
Assets/_Source/WeaponSystem/ClawsWeapon.cs
Assets/_Source/WeaponSystem/PistolWeapon.cs
Docs/VerticalSlice_ImplementationStatus.md (новый)
```
