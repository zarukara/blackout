using System;
using System.Collections;
using System.Collections.Generic;
using CombatSystem;
using EnemySystem;
using ProjectileSystem;
using UnityEngine;

namespace ArenaSystem
{
    public class ArenaWaveSpawner : MonoBehaviour
    {
        private enum EnemySpawnType
        {
            Melee,
            Ranged
        }

        [Header("Enemy Prefabs")]
        [SerializeField] private EnemyFacade meleeEnemyPrefab;
        [SerializeField] private EnemyFacade rangedEnemyPrefab;

        [Header("Enemy Runtime Dependencies")]
        [SerializeField] private Transform enemyTarget;
        [SerializeField] private ProjectilePool projectilePool;

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> spawnPoints = new();

        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnedEnemiesParent;
        [SerializeField] private float randomRadius = 0.5f;

        [Header("Waves")]
        [SerializeField] private bool startAutomatically = true;
        [SerializeField] private float firstWaveDelay = 1f;
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private List<ArenaWaveData> waves = new();

        private readonly Dictionary<Health, Action> enemyDeathHandlers = new();

        private Coroutine waveRoutine;
        private int currentWaveIndex = -1;
        private int remainingEnemiesInCurrentWave;
        private int totalEnemiesInCurrentWave;
        private bool isRunning;
        private string currentStatus = "Ожидание";

        public event Action<int, int> WaveChanged;
        public event Action<int, int> EnemyCountChanged;
        public event Action<string> StatusChanged;
        public event Action ArenaCompleted;

        public int TotalWaves => waves.Count;
        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int RemainingEnemiesInCurrentWave => remainingEnemiesInCurrentWave;
        public int TotalEnemiesInCurrentWave => totalEnemiesInCurrentWave;
        public string CurrentStatus => currentStatus;

        private void Start()
        {
            if (startAutomatically)
                StartWaves();
        }

        private void OnDisable()
        {
            StopWaves();
            ClearEnemySubscriptions();
        }

        public void StartWaves()
        {
            if (isRunning || waveRoutine != null)
                return;

            waveRoutine = StartCoroutine(RunWavesRoutine());
        }

        public void StopWaves()
        {
            if (waveRoutine != null)
                StopCoroutine(waveRoutine);

            waveRoutine = null;
            isRunning = false;
        }

        private IEnumerator RunWavesRoutine()
        {
            isRunning = true;

            SetStatus("Подготовка");
            InvokeWaveChanged(0);
            InvokeEnemyCountChanged(0, 0);

            yield return new WaitForSeconds(firstWaveDelay);

            for (currentWaveIndex = 0; currentWaveIndex < waves.Count; currentWaveIndex++)
            {
                ArenaWaveData wave = waves[currentWaveIndex];
                List<EnemySpawnType> spawnQueue = CreateSpawnQueue(wave);

                totalEnemiesInCurrentWave = spawnQueue.Count;
                remainingEnemiesInCurrentWave = 0;

                InvokeWaveChanged(currentWaveIndex + 1);
                InvokeEnemyCountChanged(remainingEnemiesInCurrentWave, totalEnemiesInCurrentWave);
                SetStatus($"Волна {currentWaveIndex + 1}");

                yield return SpawnWaveRoutine(wave, spawnQueue);

                SetStatus("Зачистите арену");

                while (remainingEnemiesInCurrentWave > 0)
                    yield return null;

                SetStatus("Волна завершена");

                if (currentWaveIndex < waves.Count - 1)
                    yield return new WaitForSeconds(timeBetweenWaves);
            }

            SetStatus("Арена очищена");
            ArenaCompleted?.Invoke();

            isRunning = false;
            waveRoutine = null;
        }

        private IEnumerator SpawnWaveRoutine(ArenaWaveData wave, List<EnemySpawnType> spawnQueue)
        {
            foreach (EnemySpawnType enemyType in spawnQueue)
            {
                SpawnEnemy(enemyType);

                if (wave.SpawnInterval > 0f)
                    yield return new WaitForSeconds(wave.SpawnInterval);
            }
        }

        private List<EnemySpawnType> CreateSpawnQueue(ArenaWaveData wave)
        {
            List<EnemySpawnType> spawnQueue = new();

            int totalEnemyCount = Mathf.Max(0, wave.TotalEnemyCount);

            if (totalEnemyCount == 0)
                return spawnQueue;

            float meleePercent = Mathf.Max(0f, wave.MeleePercent);
            float rangedPercent = Mathf.Max(0f, wave.RangedPercent);
            float totalPercent = meleePercent + rangedPercent;

            if (totalPercent <= 0f)
            {
                meleePercent = 100f;
                rangedPercent = 0f;
                totalPercent = 100f;
            }

            int meleeCount = Mathf.RoundToInt(totalEnemyCount * (meleePercent / totalPercent));
            int rangedCount = totalEnemyCount - meleeCount;

            for (int i = 0; i < meleeCount; i++)
                spawnQueue.Add(EnemySpawnType.Melee);

            for (int i = 0; i < rangedCount; i++)
                spawnQueue.Add(EnemySpawnType.Ranged);

            Shuffle(spawnQueue);

            return spawnQueue;
        }

        private void Shuffle(List<EnemySpawnType> spawnQueue)
        {
            for (int i = 0; i < spawnQueue.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, spawnQueue.Count);
                (spawnQueue[i], spawnQueue[randomIndex]) = (spawnQueue[randomIndex], spawnQueue[i]);
            }
        }

        private void SpawnEnemy(EnemySpawnType enemyType)
        {
            EnemyFacade enemyPrefab = GetEnemyPrefab(enemyType);

            if (enemyPrefab == null)
            {
                Debug.LogError($"Enemy prefab is missing for type: {enemyType}", this);
                DecreaseExpectedEnemyCount();
                return;
            }

            Transform spawnPoint = GetRandomSpawnPoint();

            if (spawnPoint == null)
            {
                Debug.LogError("Spawn points are missing.", this);
                DecreaseExpectedEnemyCount();
                return;
            }

            Vector3 spawnPosition = GetRandomizedPosition(spawnPoint.position);

            EnemyFacade enemy = Instantiate(
                enemyPrefab,
                spawnPosition,
                spawnPoint.rotation,
                spawnedEnemiesParent
            );

            EnemySpawnContext context = new(enemyTarget, projectilePool);
            enemy.Initialize(context);

            Health enemyHealth = enemy.Health;

            if (enemyHealth == null)
            {
                Debug.LogError($"Spawned enemy has no Health component: {enemy.name}", enemy);
                Destroy(enemy.gameObject);

                DecreaseExpectedEnemyCount();
                return;
            }

            remainingEnemiesInCurrentWave++;
            InvokeEnemyCountChanged(remainingEnemiesInCurrentWave, totalEnemiesInCurrentWave);

            SubscribeToEnemyDeath(enemyHealth);
        }

        private EnemyFacade GetEnemyPrefab(EnemySpawnType enemyType)
        {
            return enemyType switch
            {
                EnemySpawnType.Melee => meleeEnemyPrefab,
                EnemySpawnType.Ranged => rangedEnemyPrefab,
                _ => null
            };
        }

        private void DecreaseExpectedEnemyCount()
        {
            totalEnemiesInCurrentWave = Mathf.Max(0, totalEnemiesInCurrentWave - 1);
            InvokeEnemyCountChanged(remainingEnemiesInCurrentWave, totalEnemiesInCurrentWave);
        }

        private Transform GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
                return null;

            List<Transform> validSpawnPoints = new();

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                    validSpawnPoints.Add(spawnPoint);
            }

            if (validSpawnPoints.Count == 0)
                return null;

            return validSpawnPoints[UnityEngine.Random.Range(0, validSpawnPoints.Count)];
        }

        private Vector3 GetRandomizedPosition(Vector3 basePosition)
        {
            if (randomRadius <= 0f)
                return basePosition;

            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * randomRadius;

            return new Vector3(
                basePosition.x + randomOffset.x,
                basePosition.y,
                basePosition.z + randomOffset.y
            );
        }

        private void SubscribeToEnemyDeath(Health enemyHealth)
        {
            if (enemyDeathHandlers.ContainsKey(enemyHealth))
                return;

            Action deathHandler = () => OnEnemyDied(enemyHealth);

            enemyDeathHandlers.Add(enemyHealth, deathHandler);
            enemyHealth.Died += deathHandler;
        }

        private void OnEnemyDied(Health enemyHealth)
        {
            UnsubscribeFromEnemyDeath(enemyHealth);

            remainingEnemiesInCurrentWave = Mathf.Max(
                0,
                remainingEnemiesInCurrentWave - 1
            );

            InvokeEnemyCountChanged(
                remainingEnemiesInCurrentWave,
                totalEnemiesInCurrentWave
            );
        }

        private void UnsubscribeFromEnemyDeath(Health enemyHealth)
        {
            if (enemyHealth == null)
                return;

            if (!enemyDeathHandlers.TryGetValue(enemyHealth, out Action deathHandler))
                return;

            enemyHealth.Died -= deathHandler;
            enemyDeathHandlers.Remove(enemyHealth);
        }

        private void ClearEnemySubscriptions()
        {
            foreach (KeyValuePair<Health, Action> pair in enemyDeathHandlers)
            {
                if (pair.Key != null)
                    pair.Key.Died -= pair.Value;
            }

            enemyDeathHandlers.Clear();
        }

        private void InvokeWaveChanged(int currentWaveNumber)
        {
            WaveChanged?.Invoke(currentWaveNumber, waves.Count);
        }

        private void InvokeEnemyCountChanged(int remainingEnemies, int totalEnemies)
        {
            EnemyCountChanged?.Invoke(remainingEnemies, totalEnemies);
        }

        private void SetStatus(string status)
        {
            currentStatus = status;
            StatusChanged?.Invoke(currentStatus);
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints == null)
                return;

            Gizmos.color = Color.red;

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                    continue;

                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(
                    spawnPoint.position,
                    spawnPoint.position + spawnPoint.forward * 1.5f
                );
            }
        }
    }

    [Serializable]
    public class ArenaWaveData
    {
        [SerializeField] private string waveName = "Wave";

        [Min(0)]
        [SerializeField] private int totalEnemyCount = 3;

        [Header("Enemy Ratio")]
        [Range(0f, 100f)]
        [SerializeField] private float meleePercent = 100f;

        [Range(0f, 100f)]
        [SerializeField] private float rangedPercent = 0f;

        [Header("Timing")]
        [Min(0f)]
        [SerializeField] private float spawnInterval = 0.4f;

        public string WaveName => waveName;
        public int TotalEnemyCount => totalEnemyCount;
        public float MeleePercent => meleePercent;
        public float RangedPercent => rangedPercent;
        public float SpawnInterval => spawnInterval;
    }
}
