#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CombatSystem;
using PlayerSystem;
using ProjectileSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using WeaponSystem;
using Object = UnityEngine.Object;

namespace EnemySystem.Tests
{
    // Integration checks use the shipped Enemy prefab, real NavMesh, physics and Animator.
    // Excluded from player builds. The editor launcher opens an empty temporary scene.
    public sealed class MeleeCombatSmokeRunner : MonoBehaviour
    {
        public static Action<bool, string> Completed;
        private readonly List<string> results = new();
        private readonly List<GameObject> objects = new();
        private readonly List<string> errors = new();
        private NavMeshDataInstance navMesh;
        private GameObject inactive;
        private GameObject player;
        private Health playerHealth;
        private CharacterController controller;
        private GameObject prefab;
        private float deadline;

        private void Awake()
        {
            Application.logMessageReceived += CaptureError;
            deadline = Time.realtimeSinceStartup + 150f;
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup > deadline) Finish(false, "TIMEOUT");
        }

        private IEnumerator Start()
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Presentation/Prefabs/Enemy.prefab");
            var tests = new (string, Func<IEnumerator>)[]
            {
                ("Single: windup, one hit, recovery, repeated cycle", Single),
                ("Dash and locked-direction misses", Misses),
                ("Grab cancels windup; surviving Throw recovers", GrabThrow),
                ("Death, disable/re-enable, lost target and stale permissions", Lifecycle),
                ("Three enemies: spacing, fairness, limit and no player pushing", () => Group(3)),
                ("Eight enemies: spacing, fairness, limit and no player pushing", () => Group(8)),
                ("Eight enemies pursue moving and escaping target", MovingGroup),
                ("Obstacle detour, hit occlusion and collision-safe lunge", Obstacles),
                ("Eight enemies in a narrow passage", NarrowPassage),
                ("Configurable attack limit and expired permission recovery", Permissions),
                ("Damage callback cancellation and component lifecycle", CallbackCancellation),
                ("Claws, projectile, Grab query, Throw wall/enemy collisions", ExistingMechanics)
            };
            foreach (var test in tests)
            {
                bool failed = false;
                var stack = new Stack<IEnumerator>();
                stack.Push(test.Item2());
                while (stack.Count > 0)
                {
                    object next = null;
                    try
                    {
                        if (!stack.Peek().MoveNext()) { stack.Pop(); continue; }
                        next = stack.Peek().Current;
                    }
                    catch (Exception exception)
                    {
                        results.Add("FAIL " + test.Item1 + ": " + exception.Message);
                        failed = true;
                        break;
                    }
                    if (next is IEnumerator nested) stack.Push(nested);
                    else yield return next;
                }
                if (!failed) results.Add("PASS " + test.Item1);
                Cleanup();
                yield return null;
            }
            Finish(!results.Exists(value => value.StartsWith("FAIL")) && errors.Count == 0, null);
        }

        private void Setup(params Bounds[] walls)
        {
            Check(prefab != null, "Enemy prefab missing");
            inactive = new GameObject("Inactive factory");
            inactive.SetActive(false);
            objects.Add(inactive);
            var sources = new List<NavMeshBuildSource>();
            Box("Floor", new Vector3(0f, -0.1f, 0f), new Vector3(80f, 0.2f, 80f), 6);
            sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box,
                transform = Matrix4x4.TRS(new Vector3(0f, -0.1f, 0f), Quaternion.identity, Vector3.one), size = new Vector3(80f, 0.2f, 80f) });
            foreach (Bounds wall in walls)
            {
                Box("Wall", wall.center, wall.size, 8);
                sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(wall.center, Quaternion.identity, Vector3.one), size = wall.size });
            }
            NavMeshData data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), sources,
                new Bounds(Vector3.zero, new Vector3(90f, 10f, 90f)), Vector3.zero, Quaternion.identity);
            navMesh = NavMesh.AddNavMeshData(data);
            player = new GameObject("Player test target");
            player.SetActive(false);
            player.layer = 0;
            player.transform.position = Vector3.up * 1.08f;
            objects.Add(player);
            playerHealth = player.AddComponent<Health>();
            Set(playerHealth, "maxHealth", 10000);
            Set(playerHealth, "destroyOnDeath", false);
            controller = player.AddComponent<CharacterController>();
            controller.center = Vector3.zero;
            controller.height = 2f;
            controller.radius = 0.5f;
            CapsuleCollider playerCapsule = player.AddComponent<CapsuleCollider>();
            playerCapsule.height = 2f;
            playerCapsule.radius = 0.5f;
            player.SetActive(true);
            Physics.SyncTransforms();
        }

        private EnemyMeleeAttack Enemy(Vector3 position)
        {
            GameObject instance = Instantiate(prefab, inactive.transform);
            instance.name = "Enemy test " + objects.Count;
            objects.Add(instance);
            instance.transform.position = position;
            instance.transform.forward = Vector3.back;
            Set(instance.GetComponent<Health>(), "destroyOnDeath", false);
            instance.transform.SetParent(null);
            instance.GetComponent<AEnemy>().SetTarget(player.transform);
            instance.GetComponentInChildren<Animator>().cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return instance.GetComponent<EnemyMeleeAttack>();
        }

        private IEnumerator Single()
        {
            Setup();
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, 4f));
            int starts = 0;
            enemy.AttackPerformed += () => starts++;
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Windup, 5f, "Did not chase into windup");
            int hp = playerHealth.CurrentHealth;
            Check(hp == 10000 && starts == 1, "Damage or repeated animation trigger at attack start");
            yield return new WaitForSeconds(0.2f);
            Check(playerHealth.CurrentHealth == hp, "Windup dealt damage");
            Check(enemy.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack"), "Animator did not enter Attack during windup");
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Recovery, 2f, "No recovery");
            Check(playerHealth.CurrentHealth == hp - 10, "Stationary target must receive exactly one hit");
            yield return new WaitForSeconds(0.25f);
            Check(enemy.Phase == MeleeAttackPhase.Recovery && starts == 1 && playerHealth.CurrentHealth == hp - 10, "Recovery restarted attack or repeated damage");
            yield return Until(() => starts == 2, 4f, "Attack cycle stuck");
            var animator = enemy.GetComponentInChildren<Animator>();
            Check(!animator.applyRootMotion, "Root motion enabled");
        }

        private IEnumerator Misses()
        {
            Setup();
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Windup, 2f, "No windup");
            yield return new WaitForSeconds(0.26f);
            var dash = player.AddComponent<PlayerDashController>();
            yield return (IEnumerator)typeof(PlayerDashController).GetMethod("DashRoutine", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dash, new object[] { Vector3.right });
            yield return new WaitForSeconds(0.3f);
            Check(playerHealth.CurrentHealth == 10000, "Escaping with real Dash routine did not miss");
            enemy.GetComponent<AEnemy>().SetTarget(null);
            yield return null;
            Cleanup();
            Setup();
            enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Windup, 2f, "No windup");
            yield return new WaitForSeconds(0.26f);
            Vector3 facing = enemy.transform.forward;
            controller.Move(Vector3.forward * 3f);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.35f);
            Check(playerHealth.CurrentHealth == 10000, "Locked attack hit a target behind the enemy");
            Check(Vector3.Angle(facing, enemy.transform.forward) < 1f, "Strike tracked target after aim lock");
        }

        private IEnumerator GrabThrow()
        {
            Setup();
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Windup, 2f, "No windup");
            Transform hold = new GameObject("HoldPoint").transform;
            hold.SetParent(player.transform);
            hold.localPosition = Vector3.forward;
            var grab = enemy.GetComponent<EnemyGrabHandler>();
            grab.Grab(hold);
            Check(!enemy.IsAttacking && player.GetComponent<MeleeAttackCoordinator>().ActiveCount == 0, "Grab retained attack permission");
            Check(!enemy.GetComponent<NavMeshAgent>().enabled && enemy.GetComponent<Rigidbody>().isKinematic, "Grab physics conflict");
            yield return new WaitForSeconds(0.65f);
            Check(playerHealth.CurrentHealth == 10000, "Cancelled windup dealt damage");
            grab.Throw(Vector3.right, 2f);
            Check(!enemy.GetComponent<NavMeshAgent>().enabled && !enemy.GetComponent<Rigidbody>().isKinematic, "Throw physics conflict");
            yield return new WaitForSeconds(1.7f);
            Check(!grab.IsThrown && enemy.GetComponent<Rigidbody>().isKinematic && enemy.GetComponent<EnemyMovement>().IsReady,
                "Surviving throw failed to recover");
        }

        private IEnumerator Lifecycle()
        {
            Setup();
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.IsAttacking, 2f, "No attack");
            enemy.GetComponent<Health>().TakeDamage(1000);
            Check(!enemy.IsAttacking && player.GetComponent<MeleeAttackCoordinator>().ActiveCount == 0, "Death retained attack");
            enemy.gameObject.SetActive(false);
            yield return null;
            enemy.gameObject.SetActive(true);
            enemy.GetComponent<EnemyStateMachine>().ChangeState(EnemyStateId.Chase);
            yield return new WaitForSeconds(0.6f);
            Check(enemy.GetComponent<EnemyStateMachine>().CurrentStateId == EnemyStateId.Dead && playerHealth.CurrentHealth == 10000, "Dead enemy resumed combat");
            enemy.gameObject.SetActive(false);
            enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.IsAttacking, 2f, "No second attack");
            enemy.gameObject.SetActive(false);
            Check(player.GetComponent<MeleeAttackCoordinator>().ActiveCount == 0, "Disable retained lease");
            yield return new WaitForSeconds(0.6f);
            enemy.gameObject.SetActive(true);
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Windup, 3f, "Re-enable stuck");
            enemy.GetComponent<AEnemy>().SetTarget(null);
            Check(!enemy.IsAttacking, "Target change did not cancel immediately");
            yield return new WaitForSeconds(0.6f);
            Check(playerHealth.CurrentHealth == 10000 && player.GetComponent<MeleeAttackCoordinator>().ActiveCount == 0, "Target loss retained damage or lease");
            enemy.GetComponent<AEnemy>().SetTarget(player.transform);
            yield return Until(() => enemy.IsAttacking, 3f, "Target restoration failed");
            enemy.GetComponent<EnemyStateMachine>().enabled = false;
            Check(player.GetComponent<MeleeAttackCoordinator>().ActiveCount == 0, "Disabled state machine retained lease");
            enemy.GetComponent<EnemyStateMachine>().enabled = true;
            yield return Until(() => enemy.IsAttacking, 3f, "State machine re-enable stuck");
        }

        private IEnumerator Group(int count)
        {
            Setup();
            var enemies = new EnemyMeleeAttack[count];
            var starts = new int[count];
            for (int i = 0; i < count; i++)
            {
                enemies[i] = Enemy(new Vector3((i % 3 - 1) * 1.5f, 0f, 3f + i / 3 * 1.5f));
                int index = i;
                enemies[i].AttackPerformed += () => starts[index]++;
            }
            float end = Time.time + (count == 3 ? 10f : 17f);
            float minimumSpacing = float.PositiveInfinity;
            int peak = 0;
            Vector3 initial = player.transform.position;
            while (Time.time < end)
            {
                controller.Move(Vector3.down * Time.deltaTime);
                int active = 0;
                for (int i = 0; i < count; i++)
                {
                    if (enemies[i].IsAttacking) active++;
                    for (int j = i + 1; j < count; j++)
                        minimumSpacing = Mathf.Min(minimumSpacing, Vector3.Distance(enemies[i].transform.position, enemies[j].transform.position));
                }
                peak = Mathf.Max(peak, active);
                Check(active <= 2, "More than two simultaneous attacks");
                Vector3 drift = player.transform.position - initial;
                drift.y = 0f;
                Check(drift.magnitude < 0.03f, "Crowd physically pushed CharacterController: " + drift);
                yield return null;
            }
            Check(peak == 2, "Group never used both permissions");
            for (int i = 0; i < count; i++) Check(starts[i] > 0, "Enemy " + i + " starved: " + string.Join(",", starts));
            Check(minimumSpacing > 0.65f, "Agents collapsed into a single mass: spacing " + minimumSpacing);
            for (int i = 0; i < count; i++)
                foreach (Collider collider in enemies[i].GetComponentsInChildren<Collider>())
                    if (!collider.isTrigger) Check(Physics.GetIgnoreCollision(collider, controller), "Solid child collider still pushes player");
            results.Add("METRIC group=" + count + " starts=" + string.Join(",", starts) + " minSpacing=" + minimumSpacing.ToString("F3") + " peak=" + peak);
        }

        private IEnumerator MovingGroup()
        {
            Setup();
            var enemies = new EnemyMeleeAttack[8];
            var initial = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                initial[i] = new Vector3((i % 3 - 1) * 1.5f, 0f, -4f - i / 3 * 1.5f);
                enemies[i] = Enemy(initial[i]);
            }
            float end = Time.time + 4f;
            while (Time.time < end) { controller.Move(Vector3.forward * Time.deltaTime); yield return null; }
            for (int i = 0; i < 8; i++) Check(enemies[i].transform.position.z > initial[i].z + 1f, "Moving target left an idle enemy");
            controller.Move(Vector3.forward * 7f);
            yield return new WaitForSeconds(1.2f);
            for (int i = 0; i < 8; i++)
            {
                Check(enemies[i].GetComponent<EnemyStateMachine>().CurrentStateId == EnemyStateId.Chase, "Enemy did not chase escaping player");
                Check(Vector3.Distance(enemies[i].GetComponent<MeleeApproach>().Destination, player.transform.position) < 3f, "Stale approach destination after escape");
            }
        }

        private IEnumerator Obstacles()
        {
            Setup(new Bounds(new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 0.4f)));
            controller.Move(Vector3.forward * 3f);
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, -3f));
            float end = Time.time + 10f;
            while (!enemy.IsAttacking && Time.time < end)
            {
                Vector3 p = enemy.transform.position;
                Check(!(Mathf.Abs(p.x) < 2.3f && Mathf.Abs(p.z) < 0.4f), "Chase crossed wall");
                yield return null;
            }
            Check(enemy.IsAttacking, "Failed to navigate around wall");
            Cleanup();
            Setup();
            enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.Phase == MeleeAttackPhase.Windup, 2f, "No windup");
            Box("Sudden wall", new Vector3(0f, 1f, 0.7f), new Vector3(4f, 2f, 0.2f), 8);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.65f);
            Check(playerHealth.CurrentHealth == 10000, "Melee hit through wall");
            Check(enemy.transform.position.z >= 1.29f, "Lunge crossed physical wall");
        }

        private IEnumerator NarrowPassage()
        {
            Setup(new Bounds(new Vector3(-1.5f, 1f, 0f), new Vector3(0.4f, 2f, 20f)),
                new Bounds(new Vector3(1.5f, 1f, 0f), new Vector3(0.4f, 2f, 20f)));
            controller.Move(Vector3.forward * 4f);
            var enemies = new EnemyMeleeAttack[8];
            for (int i = 0; i < 8; i++) enemies[i] = Enemy(new Vector3(0f, 0f, -3f - i * 1.1f));
            float end = Time.time + 8f;
            int starts = 0;
            foreach (var enemy in enemies) enemy.AttackPerformed += () => starts++;
            while (Time.time < end)
            {
                controller.Move(Vector3.down * Time.deltaTime);
                Check(Mathf.Abs(player.transform.position.x) < 0.03f && Mathf.Abs(player.transform.position.z - 4f) < 0.03f,
                    "Player pushed in narrow corridor");
                foreach (var enemy in enemies)
                    if (Mathf.Abs(enemy.transform.position.z) < 9f)
                        Check(Mathf.Abs(enemy.transform.position.x) < 0.85f, "Enemy crossed corridor wall");
                yield return null;
            }
            Check(starts > 0, "No enemy traversed corridor to attack");
        }

        private IEnumerator Permissions()
        {
            Setup();
            var coordinator = player.AddComponent<MeleeAttackCoordinator>();
            Set(coordinator, "maxConcurrentAttacks", 1);
            EnemyMeleeAttack first = Enemy(new Vector3(-1.1f, 0f, 1.1f));
            EnemyMeleeAttack second = Enemy(new Vector3(1.1f, 0f, 1.1f));
            yield return Until(() => first.IsAttacking || second.IsAttacking, 3f, "No permission issued");
            EnemyMeleeAttack stuck = first.IsAttacking ? first : second;
            bool cancelled = false;
            stuck.AttackCancelled += () => cancelled = true;
            // Simulate a stalled phase while the object remains enabled and alive.
            Set(stuck, "phaseStarted", Time.time + 20f);
            float end = Time.time + 3f;
            while (Time.time < end)
            {
                Check((first.IsAttacking ? 1 : 0) + (second.IsAttacking ? 1 : 0) <= 1, "Configured limit one exceeded");
                yield return null;
            }
            Check(cancelled, "Expired permission did not cancel a stalled attack");
        }
        private IEnumerator CallbackCancellation()
        {
            Setup();
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            playerHealth.HealthChanged += (current, maximum) => enemy.gameObject.SetActive(false);
            yield return Until(() => !enemy.gameObject.activeSelf, 3f, "No damage callback");
            Check(!enemy.IsAttacking && player.GetComponent<MeleeAttackCoordinator>().ActiveCount == 0,
                "Damage callback resurrected cancelled strike");
            enemy.gameObject.SetActive(true);
            yield return Until(() => enemy.IsAttacking, 3f, "No fresh attack");
            var actor = enemy.GetComponent<AEnemy>();
            actor.enabled = false;
            enemy.GetComponent<Health>().TakeDamage(1000);
            actor.enabled = true;
            Check(enemy.GetComponent<EnemyStateMachine>().CurrentStateId == EnemyStateId.Dead, "Death while actor disabled did not restore Dead");
            Cleanup();
            Setup();
            enemy = Enemy(new Vector3(0f, 0f, 1.6f));
            yield return Until(() => enemy.IsAttacking, 3f, "No attack before grab disable");
            var hold = new GameObject("HoldPoint").transform;
            hold.SetParent(player.transform);
            hold.localPosition = Vector3.forward;
            var grab = enemy.GetComponent<EnemyGrabHandler>();
            grab.Grab(hold);
            grab.enabled = false;
            grab.enabled = true;
            yield return null;
            Check(enemy.transform.parent == null && !grab.IsGrabbed && enemy.GetComponent<EnemyMovement>().IsReady,
                "Grab component re-enable stuck");
        }
        private IEnumerator ExistingMechanics()
        {
            Setup();
            EnemyMeleeAttack enemy = Enemy(new Vector3(0f, 0f, 1.1f));
            yield return null;
            var claws = player.AddComponent<ClawsWeapon>();
            Set(claws, "enemyLayer", (LayerMask)(1 << 7));
            claws.Attack();
            Check(enemy.GetComponent<Health>().CurrentHealth == 50, "Claws query lost hitbox or applied duplicate damage");
            var grab = player.AddComponent<PlayerGrabController>();
            Transform hold = new GameObject("HoldPoint").transform;
            hold.SetParent(player.transform);
            hold.localPosition = Vector3.forward;
            Set(grab, "grabPoint", player.transform);
            Set(grab, "holdPoint", hold);
            Set(grab, "enemyLayer", (LayerMask)(1 << 7));
            typeof(PlayerGrabController).GetMethod("TryGrab", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(grab, null);
            Check(grab.HasGrabbedEnemy && enemy.GetComponent<EnemyGrabHandler>().IsGrabbed, "Grab overlap lost hitbox");
            Box("Throw wall", new Vector3(0f, 1f, 3f), new Vector3(5f, 2f, 0.3f), 8);
            enemy.GetComponent<EnemyGrabHandler>().Throw(Vector3.forward, 8f);
            yield return new WaitForSeconds(0.5f);
            Check(enemy.GetComponent<Health>().IsDead, "Thrown enemy did not collide with wall");
            Cleanup();
            Setup();
            enemy = Enemy(new Vector3(0f, 0f, 1.1f));
            EnemyMeleeAttack other = Enemy(new Vector3(0f, 0f, 3f));
            other.GetComponent<AEnemy>().SetTarget(null);
            yield return null;
            hold = new GameObject("HoldPoint").transform;
            hold.SetParent(player.transform);
            hold.localPosition = Vector3.forward;
            enemy.GetComponent<EnemyGrabHandler>().Grab(hold);
            enemy.GetComponent<EnemyGrabHandler>().Throw(Vector3.forward, 8f);
            yield return new WaitForSeconds(0.5f);
            Check(other.GetComponent<Health>().CurrentHealth == 25 && enemy.GetComponent<Health>().IsDead, "Throw vs enemy collision changed");
            Cleanup();
            Setup();
            enemy = Enemy(new Vector3(0f, 0f, 2f));
            yield return null;
            var poolObject = new GameObject("Test projectile pool");
            poolObject.SetActive(false);
            objects.Add(poolObject);
            var pool = poolObject.AddComponent<ProjectilePool>();
            Set(pool, "initialSize", 0);
            poolObject.SetActive(true);
            var bulletObject = new GameObject("Test projectile");
            bulletObject.SetActive(false);
            objects.Add(bulletObject);
            bulletObject.AddComponent<SphereCollider>().isTrigger = true;
            var bullet = bulletObject.AddComponent<Projectile>();
            Set(bullet, "hitLayers", (LayerMask)(1 << 7));
            bulletObject.SetActive(true);
            bullet.Initialize(pool);
            bullet.Launch(Vector3.up, Vector3.forward, 8f, 15, player);
            yield return new WaitForSeconds(0.5f);
            Check(enemy.GetComponent<Health>().CurrentHealth == 85 && !bulletObject.activeSelf, "Projectile did not hit exactly once and return to pool");
        }

        private static IEnumerator Until(Func<bool> condition, float timeout, string message)
        {
            float end = Time.time + timeout;
            while (!condition() && Time.time < end) yield return null;
            Check(condition(), message);
        }

        private GameObject Box(string name, Vector3 position, Vector3 size, int layer)
        {
            var box = new GameObject(name);
            box.layer = layer;
            box.transform.position = position;
            box.AddComponent<BoxCollider>().size = size;
            objects.Add(box);
            return box;
        }

        private static void Set(object target, string field, object value)
        {
            FieldInfo info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            if (info == null) throw new Exception("Missing test field: " + field);
            info.SetValue(target, value);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private void Cleanup()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) DestroyImmediate(objects[i]);
            objects.Clear();
            if (navMesh.valid) navMesh.Remove();
        }

        private void CaptureError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                errors.Add(type + ": " + message + "\n" + stack);
        }

        private void Finish(bool success, string extra)
        {
            enabled = false;
            StopAllCoroutines();
            Application.logMessageReceived -= CaptureError;
            Cleanup();
            string report = string.Join("\n", results) + "\nErrors: " + errors.Count + "\n" + string.Join("\n", errors) + extra;
            Completed?.Invoke(success, report);
        }
    }
}
#endif


