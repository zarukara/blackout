using ArenaSystem;
using TMPro;
using UnityEngine;

namespace UISystem
{
    public class ArenaWaveUiView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArenaWaveSpawner waveSpawner;

        [Header("Texts")]
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text enemiesText;
        [SerializeField] private TMP_Text statusText;

        private bool isSubscribed;

        private void Awake()
        {
            if (waveSpawner == null)
                waveSpawner = FindFirstObjectByType<ArenaWaveSpawner>();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshView();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (isSubscribed)
                return;

            if (waveSpawner == null)
                return;

            waveSpawner.WaveChanged += OnWaveChanged;
            waveSpawner.EnemyCountChanged += OnEnemyCountChanged;
            waveSpawner.StatusChanged += OnStatusChanged;
            waveSpawner.ArenaCompleted += OnArenaCompleted;

            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            if (waveSpawner == null)
                return;

            waveSpawner.WaveChanged -= OnWaveChanged;
            waveSpawner.EnemyCountChanged -= OnEnemyCountChanged;
            waveSpawner.StatusChanged -= OnStatusChanged;
            waveSpawner.ArenaCompleted -= OnArenaCompleted;

            isSubscribed = false;
        }

        private void RefreshView()
        {
            if (waveSpawner == null)
            {
                OnWaveChanged(0, 0);
                OnEnemyCountChanged(0, 0);
                OnStatusChanged("Ожидание");
                return;
            }

            OnWaveChanged(
                Mathf.Max(0, waveSpawner.CurrentWaveNumber),
                waveSpawner.TotalWaves
            );

            OnEnemyCountChanged(
                waveSpawner.RemainingEnemiesInCurrentWave,
                waveSpawner.TotalEnemiesInCurrentWave
            );

            OnStatusChanged(waveSpawner.CurrentStatus);
        }

        private void OnWaveChanged(int currentWave, int totalWaves)
        {
            if (waveText == null)
                return;

            waveText.text = $"Волна: {currentWave}/{totalWaves}";
        }

        private void OnEnemyCountChanged(int remainingEnemies, int totalEnemies)
        {
            if (enemiesText == null)
                return;

            enemiesText.text = $"Враги: {remainingEnemies}/{totalEnemies}";
        }

        private void OnStatusChanged(string status)
        {
            if (statusText == null)
                return;

            statusText.text = status;
        }

        private void OnArenaCompleted()
        {
            OnStatusChanged("Арена очищена");
            OnEnemyCountChanged(0, 0);
        }
    }
}