# Melee Combat — реализация

Дата: 11.10.2026. Unity: 6000.3.8f1. Задание: приложенный «BLACKHOST — MELEE ENEMY COMBAT REWORK».

## Область и аудит

Работа выполнена поверх чистого checkout после отмены прежних экспериментов. На момент этого аудита корневого `AGENTS.md` и `Docs/VerticalSlice_Functionality.md` в checkout нет. Источник требований — новое приложенное melee-задание. Старые экспериментальные системы не использовались.

Изучены AEnemy, EnemyActor, EnemyContext, EnemyFacade, EnemyStateMachine, все enemy states, EnemyMovement, EnemyMeleeAttack, EnemyAnimationController, EnemyGrabHandler, ThrownEnemyDamage, Health, PlayerMovement/Dash/Grab, ClawsWeapon, Projectile/Pool, ArenaWaveSpawner, существующие Enemy/Player prefab и melee Animator. Авторских asmdef и VContainer в данном checkout нет; сохранены существующие namespaces и способ связывания компонентов.

Исходные проблемы: TryAttack сразу вызывал TakeDamage; melee не имел фаз; каждый NavMeshAgent строил путь к Transform игрока каждый кадр; переход Any State → Attack занимал 0,7 с. Enemy имеет корневую CapsuleCollider и дочернюю коллизию Baton. Player имеет одновременно CapsuleCollider и CharacterController, центр которых находится в Transform игрока; Transform находится примерно на метр выше поверхности. Матрица слоёв разрешает контакты всех слоёв. Поэтому исключение только слоя Enemy или расчёт высоты/точек NavMesh непосредственно из Player.position были бы недостаточны.

## Реализованное поведение

- Chase использует прежний EnemyMovement. Новый MoveToPosition обновляет destination не чаще 0,2 с при заметном изменении точки/потере пути; Stop не вызывает ResetPath каждый кадр. Обычные переходы состояния больше не вызывают Warp. Существующее восстановление позиции на NavMesh после Throw сохранено.
- MeleeApproach учитывает скорость цели, прогноз на 0,15 с и локальное разделение. При скорости выше 0,8 м/с все враги преследуют цель. Возврат к спокойному сближению: скорость ниже 0,3 м/с в течение 0,35 с. Побег цели на 0,8 м немедленно обновляет назначение.
- У остановившейся цели каждый враг оценивает небольшой набор доступных точек со своей стороны. Учитываются полный NavMesh-путь, позиции соседей, их назначенные точки и сохранение предыдущего назначения. Нет фиксированных секторов, ролей и равномерного построения кольца. Обычный пересчёт — раз в 0,3 с.
- MeleeAttackCoordinator находится на цели и отвечает только за разрешения. По умолчанию 2 активные атаки, включая Recovery. Очередь даёт возможность следующим врагам; застрявшее сближение теряет приоритет через 2,5 с. Необновляемые заявки истекают через 0,75 с; активное разрешение — через длительность цикла плюс 1 с. Записи разрешений переиспользуются.
- Windup 0,35 с: остановка агента, один AttackPerformed/Animator trigger, ограниченный поворот 180°/с. После 65% замаха направление фиксируется. Урона нет.
- Strike 0,15 с: необязательный выпад 0,4 м через NavMeshAgent.Move. NavMesh.Raycast и CapsuleCast ограничивают движение стенами/другими телами. Одна контактная проверка при 50% Strike: текущая дистанция, высота тела, сектор 90°, видимость. Вышедшая из сектора/дальности цель получает промах. По умолчанию контакт примерно через 0,425 с от старта, с точностью до кадров.
- Recovery 0,5 с: движение и повторная атака заблокированы. Затем разрешение освобождается, начинается Chase. Старый Attack Cooldown сохранён как дополнительное ограничение частоты начала атак.
- Grab/Throw, смерть, смена/потеря цели, отключение объекта, actor или state machine отменяют melee. Возврат мёртвого melee в Chase запрещён. Прерванная recovery-корутина Throw очищается; при восстановлении больше не остаётся активного окна thrown-урона. Отключение/включение компонента Grab также освобождает захват.
- MeleePlayerContact исключает только пары собственных твёрдых коллайдеров Enemy (включая Baton) с твёрдыми коллайдерами тела цели, включая CharacterController и CapsuleCollider. Старые значения IgnoreCollision сохраняются и восстанавливаются. Контакты проверяются повторно после временного отключения коллайдеров. Коллайдеры остаются доступны пулям, когтям и Grab; Throw по стенам и другим врагам сохранён. Глобальная Layer Collision Matrix не менялась.

## Изменённые файлы

Пути ниже относительно корня проекта.

| Файл | Изменение |
|---|---|
| `Assets/_Source/EnemySystem/Combat/EnemyMeleeAttack.cs` | Фазы, тайминги, контакт, поворот, отмена, подключение разрешений и сближения; сохранены TryAttack, CanAttack, AttackDistance, AttackPerformed |
| `Assets/_Source/EnemySystem/Movement/EnemyMovement.cs` | MoveToPosition, безопасный MoveAttack, IsReady; отсутствие Warp при штатных переходах |
| `Assets/_Source/EnemySystem/States/EnemyChaseState.cs` | Делегирование melee-преследования; ranged-ветка сохранена |
| `Assets/_Source/EnemySystem/States/EnemyMeleeAttackState.cs` | Выполнение фаз вместо моментального повторяемого TryAttack |
| `Assets/_Source/EnemySystem/Core/EnemyContext.cs` | Немедленная отмена при смене цели |
| `Assets/_Source/EnemySystem/Core/EnemyStateMachine.cs` | Очистка/восстановление melee при отключении; защита Dead |
| `Assets/_Source/EnemySystem/Core/AEnemy.cs` | Отмена при отключении actor; смерть при временно выключенном actor |
| `Assets/_Source/EnemySystem/Core/EnemyAnimationController.cs` | Сброс trigger и выход из Attack при отмене, root motion off для melee |
| `Assets/_Source/EnemySystem/Grab/EnemyGrabHandler.cs` | Очистка прерванного melee Grab/Throw, защита мёртвой цели |
| `Assets/_Presentation/Animations/Enemies/MeleePolice/PoliceMelee Animator.controller` | Два изменения: Transition Duration 0,7 → 0,05; Attack Speed 1 → 1,5 |

Добавлены:

| Файл | Ответственность |
|---|---|
| `Assets/_Source/EnemySystem/Combat/MeleeAttackCoordinator.cs` | Очередь и ограничение разрешений на одну цель |
| `Assets/_Source/EnemySystem/Combat/MeleePlayerContact.cs` | Адресные исключения контактов тел |
| `Assets/_Source/EnemySystem/Movement/MeleeApproach.cs` | Индивидуальная навигационная точка и локальное разделение |
| `Assets/_Source/EnemySystem/Tests/MeleeCombatSmokeRunner.cs` | Интеграционные coroutine-проверки в настоящем PlayMode; код исключён из player build |
| `Assets/_Source/EnemySystem/Tests/Editor/MeleeCombatValidation.cs` | Меню запуска и batch entry point; отдельная явная утилита настройки Animator |
| `Docs/MeleeCombat_Implementation.md` | Этот отчёт |
| `Docs/VerticalSlice_ImplementationStatus.md` | Указатель на актуальную область работ |
| `Docs/Validation/MeleeCombatValidation.txt` | Сохранённый итог последнего прогона |

У новых Unity-скриптов/каталогов есть `.meta`. GUID существующих ассетов сохранены. EnemyActor, EnemyFacade, Health, оружие и управление игрока, RangedPolice/RangeEnemy, волны, уровень, модели, клипы и матрица слоёв не переписывались. Commit не создавался.

## Настройки Inspector

Для запуска ручное подключение новых компонентов **не требуется**. EnemyMeleeAttack добавляет отсутствующие MeleeApproach и MeleePlayerContact на собственный объект; координатор создаётся на Target. Поэтому уже существующие экземпляры и спавнер используют поведение сразу после компиляции.

Для постоянной настройки открывайте **Assets/_Presentation/Prefabs/Enemy.prefab → Enemy** в Prefab Mode:

| Компонент / поле | Значение |
|---|---|
| Enemy Movement / Move Speed | Сохранено 3; для предложенного баланса можно поставить 3,5 |
| Enemy Melee Attack / Attack Distance | Сохранено сериализованное 1,7; новое значение компонента по умолчанию 1,8 |
| Enemy Melee Attack / Damage | Сохранено 10 |
| Enemy Melee Attack / Attack Cooldown | Сохранено 1 |
| Windup Duration / Strike Duration / Recovery Duration | 0,35 / 0,15 / 0,5 с |
| Aim Lock Fraction / Windup Turn Speed | 0,65 / 180 |
| Contact Fraction / Attack Angle / Max Height Difference | 0,5 / 90 / 1 |
| Enable Lunge / Lunge Distance | true / 0,4 м; false полностью отключает выпад |
| Obstruction Layers | Everything; triggers игнорируются, собственное тело и цель фильтруются кодом |

Если нужно сохранять параметры сближения, добавьте **MeleeApproach** на `Enemy` через Add Component (runtime не создаст дубликат): Refresh Interval 0,3; Waiting Distance 2,7; Personal Space 1,25; Moving Speed 0,8; Stationary Speed 0,3; Prediction Time 0,15. Agent использует High Quality avoidance и различные priorities 35–64. Радиус 0,5 и размер исходной капсулы сохраняются. Arrival distance для индивидуальной точки задаётся кодом 0,12; исходный Stopping Distance используется прежними другими сценариями движения.

Для постоянной настройки лимита: **Assets/_Presentation/Prefabs/Player.prefab → Player → Add Component → MeleeAttackCoordinator**: Max Concurrent Attacks 2, Approach Timeout 2,5. Это необязательно; такие же значения уже действуют автоматически. В PlayMode компонент виден на цели, указанной спавнером. Менять слои Player/Enemy, делать все тела trigger или отключать коллайдеры не требуется.

## Animator и синхронизация

Контроллер: `Assets/_Presentation/Animations/Enemies/MeleePolice/PoliceMelee Animator.controller`, **Base Layer**:

- **Any State → Attack**: Transition Duration **0,05**, Fixed Duration **on**, Has Exit Time **off**, Can Transition To Self **off**; условие Attack сохранено.
- **Attack**: Speed **1,5**. Существующий клип имеет длину 1,5 с; это даёт цикл около 1 с. Unity сериализует значение как 1.5000001.
- **Attack → IdleCombat**: исходные Exit Time **0,9333334**, Fixed Duration **0,05** сохранены.
- **Enemy/View → Animator → Apply Root Motion**: off, как в исходном prefab, дополнительно обеспечивается для melee кодом.
- Параметры Speed, IsInCombat, Attack и все существующие clips сохранены. Animation Event не требуется; попадание использует кодовый fallback.

Эти настройки уже внесены. Меню **BlackHost → Validation → Apply Melee Attack Timing** повторяет только две правки Animator и сохраняет ассет; для обычного запуска его нажимать не нужно. После изменения полного игрового цикла настройте Attack Speed = 1,5 / (Windup + Strike + Recovery). Визуальное совпадение дубинки с контактом нужно проверить в игровой камере: начать с Windup 0,35 и Contact Fraction 0,5, при необходимости сдвигать Contact Fraction в пределах 0…1, затем Windup Duration. Headless-проверка подтверждает запуск настоящего Animator во время Windup, но не заменяет оценку визуального контакта и читаемости клипа.

## Проверки и ограничения

Unity Editor запущен реально, в batchmode/nographics, в отдельной копии `Temp/MeleeValidation`. В неё перенесены весь текущий `_Source`, неизменённые Packages manifest/lock, версия Editor, настройки физики/слоёв/NavMesh и существующие prefab с их зависимостями. Исходная открытая сцена не переключалась. Использован настоящий Enemy prefab с оригинальным Animator; Player-тест воспроизводит геометрию/высоту двух исходных коллайдеров. Dash, Claws, Grab, Projectile и ThrownEnemyDamage выполняются существующими классами. Runtime NavMesh и физические препятствия создаются тестовым runner в пустой сцене.

Это собственные интеграционные smoke tests в PlayMode, не NUnit/Test Runner XML и не полный прогон Level_01. Итоговые данные — в `Docs/Validation/MeleeCombatValidation.txt`. Лог Editor — `Logs/MeleeCombat-Unity-verified.log` (ignored Git). При первом импорте временного проекта VFX Graph выдал предупреждения `The referenced script ... is missing` из package `VFXGraphPreprocessor`; это не новый melee-компонент. Ошибки/исключения/Assert, возникающие во время проверок, учитываются runner отдельно.

Финальный прогон: **PASS, 12 из 12 сценариев; Errors/Exceptions/Asserts = 0**. Три врага: число начатых атак 4/5/4 за 10 с. Восемь: 3/3/3/3/3/3/2/2 за 17 с. Пиковое число активных атак — 2, минимальное измеренное расстояние между центрами — 1,000 м. Во всём прогоне с неподвижным игроком планарный сдвиг CharacterController оставался ниже порога 0,03 м. Дополнительно пройдены лимит 1 и проход шириной 2,6 м.

Автоматические сценарии: один враг и повтор атаки; Dash и уход за спину после фиксации; Grab во время Windup и пережитый Throw; смерть/отключение/смена цели; 3 и 8 врагов с проверкой лимита, отсутствия starvation и физического сдвига CharacterController; преследование движущейся/сбежавшей цели; обход стены, закрытие цели стеной после старта замаха, ограничение выпада; 8 врагов в узком коридоре; лимит 1 и истечение зависшего разрешения; отмена из damage callback и отключение компонентов; сохранность когтей, пуль, Grab, Throw по стене/другому врагу.

Повторить: **BlackHost → Validation → Run Melee Combat Smoke Tests**. Меню предлагает сохранить изменённую сцену, открывает пустую тестовую сцену и входит в PlayMode; результат пишет в `Logs/MeleeCombatValidation.txt`. После прогона заново откройте Level_01. Batch entry point: `-batchmode -nographics -projectPath <копия проекта> -executeMethod EnemySystem.Tests.Editor.MeleeCombatValidation.Run -logFile <лог>`; не добавлять `-quit`, runner завершает Editor после асинхронных проверок.

Не подтверждены: субъективное ощущение толпы/анимации в игровой камере, полный Level_01 с UI/волнами/смешанной ranged-группой, все варианты геометрии реального уровня, профилирование больших толп. Локальное разделение — мягкое предпочтение; в тесноте NavMesh avoidance может замедлять продвижение. Если Throw заканчивается вне доступного NavMesh дальше исходного NavMesh Search Radius (2 м), прежняя процедура восстановления может не найти позицию; этот общий случай движения не переписывался.

## Ручной прогон на существующем уровне

1. Откройте **Assets/_Presentation/Scenes/Level_01.unity**. Существующая поверхность: Hierarchy **=== Systems ===/NavMesh → NavMesh Surface**; NavMesh Data уже назначен. Уровень и bake не изменены.
2. Для удобства сначала сохраните тестовую копию сцены. В Hierarchy **Spawner → Arena Wave Spawner → Waves** оставьте одну запись: **Total Enemy Count = 1**, **Melee Percent = 100**, **Ranged Percent = 0**, **Spawn Interval = 0,3**. Поля Melee Enemy Prefab (`Enemy`), Enemy Target (`Player`), Spawn Points и Spawned Enemies Parent (`EnemiesContainer`) оставить существующими. Это временная настройка проверки; рабочая сцена в репозитории не изменена.
3. Play: стойте, убедитесь в читаемом замахе без раннего урона; уходите/Dash во время замаха, атакуйте когтями в Recovery. Посмотрите совпадение визуального контакта дубинки с изменением Health.
4. Повторите с **Total Enemy Count = 3**, затем **8**. Стойте, бегите по арене, резко остановитесь и выйдите из толпы. Враги должны преследовать, выбирать разные доступные точки и чередовать атаки; игрок не должен смещаться от контакта тел. Ожидающие стремятся к 2,7 м, но не обязаны мгновенно достигать дистанции в узком месте.
5. Проверьте Grab на замахе, Throw в стену и другого врага, выживший слабый бросок, смерть на замахе. В Hierarchy **EnemiesContainer/Enemy(Clone)** можно отключить/включить GameObject во время атаки: старый Strike не должен доиграть с уроном. Мёртвый объект при Destroy On Death=false должен остаться Dead.
6. Повторите погоню у существующих стен и в проходах. Проверьте прохождение игрока между телами, пистолет и когти. После проверки откройте исходную сцену; временный баланс волн не переносить автоматически.

