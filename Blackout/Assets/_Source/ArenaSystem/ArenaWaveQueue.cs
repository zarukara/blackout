using System.Collections.Generic;
using UnityEngine;

namespace ArenaSystem
{
    internal static class ArenaWaveQueue
    {
        public static List<EnemySpawnType> Create(ArenaWaveData wave)
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

        private static void Shuffle(List<EnemySpawnType> spawnQueue)
        {
            for (int i = 0; i < spawnQueue.Count; i++)
            {
                int randomIndex = Random.Range(i, spawnQueue.Count);
                (spawnQueue[i], spawnQueue[randomIndex]) = (spawnQueue[randomIndex], spawnQueue[i]);
            }
        }
    }
}
