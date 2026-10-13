using System;
using System.Collections;
using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyMeleeAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackDistance = 1.7f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField, Min(0f)] private float impactDelay = 0.4f;
        [SerializeField, Min(0f)] private float animationDuration = 1f;

        private float nextAttackTime;
        private Coroutine attackRoutine;
        private EnemyContext context;
        public bool IsAttacking { get; private set; }

        public float AttackDistance => attackDistance;

        public event Action AttackPerformed;
        public event Action AttackCancelled;

        private void Awake() => context = GetComponent<EnemyContext>();
        private void OnDisable() => CancelAttack();

        public void CancelAttack()
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = null;
            if (!IsAttacking) return;
            IsAttacking = false;
            AttackCancelled?.Invoke();
        }

        public bool CanAttack(Transform target)
        {
            if (target == null)
                return false;

            Vector3 direction =
                target.position - transform.position;

            direction.y = 0f;

            return direction.sqrMagnitude <=
                   attackDistance * attackDistance;
        }

        public void TryAttack(Transform target)
        {
            if (target == null || IsAttacking || Time.timeScale <= 0f || !isActiveAndEnabled)
                return;

            if (Time.time < nextAttackTime)
                return;

            if (!CanAttack(target))
                return;
            if (!CombatAim.IsFacing(transform, target.position, 15f)) return;

            Health targetHealth =
                target.GetComponent<Health>();

            if (targetHealth == null)
                return;

            nextAttackTime =
                Time.time + attackCooldown;

            IsAttacking = true;
            AttackPerformed?.Invoke();
            if (IsAttacking && isActiveAndEnabled) attackRoutine = StartCoroutine(ResolveAttack(target, targetHealth, transform.forward));
        }

        private IEnumerator ResolveAttack(Transform target, Health targetHealth, Vector3 direction)
        {
            yield return new WaitForSeconds(impactDelay);
            if (target != null && target.gameObject.activeInHierarchy && targetHealth != null && !targetHealth.IsDead &&
                (context == null || (!context.Health.IsDead && context.Target == target)) && CanAttack(target))
            {
                Vector3 toTarget = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
                if (Vector3.Angle(direction, toTarget) <= 45f) targetHealth.TakeDamage(damage);
            }
            yield return new WaitForSeconds(Mathf.Max(0f, animationDuration - impactDelay));
            IsAttacking = false;
            attackRoutine = null;
        }
    }
}
