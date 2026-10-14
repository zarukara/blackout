# BlackHost: combat / animation integration

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
