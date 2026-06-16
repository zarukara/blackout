using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    public class EnemyFacade : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Health health;
        [SerializeField] private EnemyContext context;

        [Header("Combat")]
        [SerializeField] private EnemyMeleeAttack meleeAttack;
        [SerializeField] private EnemyRangedAttack rangedAttack;

        public Health Health => health;
        public EnemyContext Context => context;
        public EnemyMeleeAttack MeleeAttack => meleeAttack;
        public EnemyRangedAttack RangedAttack => rangedAttack;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        [ContextMenu("Cache References")]
        public void CacheReferences()
        {
            if (health == null)
                health = GetComponentInChildren<Health>(true);

            if (context == null)
                context = GetComponentInChildren<EnemyContext>(true);

            if (meleeAttack == null)
                meleeAttack = GetComponentInChildren<EnemyMeleeAttack>(true);

            if (rangedAttack == null)
                rangedAttack = GetComponentInChildren<EnemyRangedAttack>(true);
        }

        public void Initialize(EnemySpawnContext spawnContext)
        {
            if (context != null)
                context.SetTarget(spawnContext.Target);

            if (rangedAttack != null)
                rangedAttack.Initialize(spawnContext.ProjectilePool);
        }

        public bool IsValid()
        {
            return health != null
                   && context != null;
        }
    }
}
