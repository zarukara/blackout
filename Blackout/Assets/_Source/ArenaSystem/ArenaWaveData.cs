using System;
using UnityEngine;

namespace ArenaSystem
{
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
