using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyContext : MonoBehaviour
    {
        public AEnemy Enemy { get; private set; }
        public Transform Target { get; private set; }

        public Health Health { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public EnemyStateMachine StateMachine { get; private set; }
        public EnemyMovement Movement { get; private set; }
        public EnemyMeleeAttack MeleeAttack { get; private set; }
        public EnemyGrabHandler GrabHandler { get; private set; }
        public ThrownEnemyDamage ThrownEnemyDamage { get; private set; }

        public void Initialize(AEnemy enemy, Transform target)
        {
            Enemy = enemy;
            Target = target;

            Health = GetComponent<Health>();
            CharacterController = GetComponent<CharacterController>();
            Rigidbody = GetComponent<Rigidbody>();

            StateMachine = GetComponent<EnemyStateMachine>();
            Movement = GetComponent<EnemyMovement>();
            MeleeAttack = GetComponent<EnemyMeleeAttack>();
            GrabHandler = GetComponent<EnemyGrabHandler>();
            ThrownEnemyDamage = GetComponent<ThrownEnemyDamage>();
        }

        public void SetTarget(Transform target)
        {
            Target = target;
        }

        public bool HasTarget()
        {
            return Target != null;
        }

        public float GetDistanceToTarget()
        {
            if (Target == null)
                return float.MaxValue;

            Vector3 direction = Target.position - transform.position;
            direction.y = 0f;

            return direction.magnitude;
        }
    }
}