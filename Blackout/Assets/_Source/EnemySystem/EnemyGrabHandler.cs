using System.Collections;
using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(Health))]
    public class EnemyGrabHandler : MonoBehaviour
    {
        [Header("Grab")]
        [SerializeField] private Vector3 localHoldPosition = Vector3.zero;
        [SerializeField] private Vector3 localHoldRotation = Vector3.zero;

        [Header("Recovery")]
        [SerializeField] private float recoveryDelay = 1.5f;

        private EnemyChase enemyChase;
        private EnemyMeleeAttack enemyMeleeAttack;
        private CharacterController characterController;
        private Rigidbody rigidbodyComponent;
        private Health health;
        private ThrownEnemyDamage thrownEnemyDamage;

        private Coroutine recoveryCoroutine;

        public bool IsGrabbed { get; private set; }
        public bool IsThrown { get; private set; }
        public Health Health => health;

        private void Awake()
        {
            enemyChase = GetComponent<EnemyChase>();
            enemyMeleeAttack = GetComponent<EnemyMeleeAttack>();
            characterController = GetComponent<CharacterController>();
            rigidbodyComponent = GetComponent<Rigidbody>();
            health = GetComponent<Health>();
            thrownEnemyDamage = GetComponent<ThrownEnemyDamage>();

            PrepareRigidbody();
        }

        public void Grab(Transform holdPoint)
        {
            if (IsGrabbed || IsThrown)
                return;

            IsGrabbed = true;
            IsThrown = false;

            if (recoveryCoroutine != null)
            {
                StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
            }

            DisableEnemyLogic();

            if (rigidbodyComponent != null)
            {
                if (!rigidbodyComponent.isKinematic)
                {
                    rigidbodyComponent.linearVelocity = Vector3.zero;
                    rigidbodyComponent.angularVelocity = Vector3.zero;
                }

                rigidbodyComponent.isKinematic = true;
                rigidbodyComponent.useGravity = false;
            }

            transform.SetParent(holdPoint);
            transform.localPosition = localHoldPosition;
            transform.localEulerAngles = localHoldRotation;
        }

        public void Throw(Vector3 direction, float force)
        {
            if (!IsGrabbed)
                return;

            IsGrabbed = false;
            IsThrown = true;

            transform.SetParent(null);

            if (rigidbodyComponent != null)
            {
                rigidbodyComponent.isKinematic = false;
                rigidbodyComponent.useGravity = false;

                rigidbodyComponent.linearVelocity = Vector3.zero;
                rigidbodyComponent.angularVelocity = Vector3.zero;

                rigidbodyComponent.AddForce(direction.normalized * force, ForceMode.Impulse);
            }

            if (thrownEnemyDamage != null)
            {
                thrownEnemyDamage.Activate();
            }

            recoveryCoroutine = StartCoroutine(RecoverAfterThrow());
        }

        private IEnumerator RecoverAfterThrow()
        {
            yield return new WaitForSeconds(recoveryDelay);

            if (health == null || health.IsDead)
                yield break;

            IsThrown = false;

            if (rigidbodyComponent != null)
            {
                if (!rigidbodyComponent.isKinematic)
                {
                    rigidbodyComponent.linearVelocity = Vector3.zero;
                    rigidbodyComponent.angularVelocity = Vector3.zero;
                }

                rigidbodyComponent.isKinematic = true;
                rigidbodyComponent.useGravity = false;
            }

            EnableEnemyLogic();
            recoveryCoroutine = null;
        }

        private void DisableEnemyLogic()
        {
            if (enemyChase != null)
                enemyChase.enabled = false;

            if (enemyMeleeAttack != null)
                enemyMeleeAttack.enabled = false;

            if (characterController != null)
                characterController.enabled = false;
        }

        private void EnableEnemyLogic()
        {
            if (characterController != null)
                characterController.enabled = true;

            if (enemyChase != null)
                enemyChase.enabled = true;

            if (enemyMeleeAttack != null)
                enemyMeleeAttack.enabled = true;
        }

        private void PrepareRigidbody()
        {
            if (rigidbodyComponent == null)
                return;

            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }
    }
}