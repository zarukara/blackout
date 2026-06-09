using System.Collections;
using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyStateMachine))]
    public class EnemyGrabHandler : MonoBehaviour
    {
        [Header("Grab")]
        [SerializeField] private Vector3 localHoldPosition = Vector3.zero;
        [SerializeField] private Vector3 localHoldRotation = Vector3.zero;

        [Header("Recovery")]
        [SerializeField] private float recoveryDelay = 1.5f;

        private EnemyStateMachine stateMachine;
        private Rigidbody rigidbodyComponent;
        private Health health;
        private ThrownEnemyDamage thrownEnemyDamage;

        private Coroutine recoveryCoroutine;

        public bool IsGrabbed { get; private set; }
        public bool IsThrown { get; private set; }
        public Health Health => health;

        private void Awake()
        {
            stateMachine = GetComponent<EnemyStateMachine>();
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

            stateMachine.ChangeState(EnemyStateId.Grabbed);

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

            stateMachine.ChangeState(EnemyStateId.Thrown);

            if (rigidbodyComponent != null)
            {
                rigidbodyComponent.isKinematic = false;
                rigidbodyComponent.useGravity = false;

                rigidbodyComponent.linearVelocity = Vector3.zero;
                rigidbodyComponent.angularVelocity = Vector3.zero;

                rigidbodyComponent.AddForce(direction.normalized * force, ForceMode.Impulse);
            }

            if (thrownEnemyDamage != null)
                thrownEnemyDamage.Activate();

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

            stateMachine.ChangeState(EnemyStateId.Chase);
            recoveryCoroutine = null;
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