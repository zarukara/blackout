# BlackHost: combat / animation integration

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
