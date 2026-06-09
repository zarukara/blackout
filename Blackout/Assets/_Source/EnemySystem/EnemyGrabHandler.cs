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

        private EnemyChase enemyChase;
        private EnemyMeleeAttack enemyMeleeAttack;
        private CharacterController characterController;
        private Rigidbody rigidbodyComponent;
        private Health health;
        private ThrownEnemyDamage thrownEnemyDamage;

        public bool IsGrabbed { get; private set; }
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
            if (IsGrabbed)
                return;

            IsGrabbed = true;

            if (enemyChase != null)
                enemyChase.enabled = false;

            if (enemyMeleeAttack != null)
                enemyMeleeAttack.enabled = false;

            if (characterController != null)
                characterController.enabled = false;

            if (rigidbodyComponent != null)
            {
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