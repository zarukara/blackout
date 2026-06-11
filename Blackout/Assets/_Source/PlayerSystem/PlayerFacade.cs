using CombatSystem;
using UnityEngine;
using WeaponSystem;

namespace PlayerSystem
{
    [DisallowMultipleComponent]
    public class PlayerFacade : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Health health;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerRotation rotation;
        [SerializeField] private PlayerGrabController grabController;
        [SerializeField] private PlayerDashController dashController;
        [SerializeField] private PlayerTargetingController targetingController;

        [Header("Weapons")]
        [SerializeField] private PlayerWeaponCollector weaponCollector;
        [SerializeField] private PlayerWeaponController weaponController;

        public Health Health => health;
        public PlayerMovement Movement => movement;
        public PlayerRotation Rotation => rotation;
        public PlayerGrabController GrabController => grabController;
        public PlayerDashController DashController => dashController;
        public PlayerTargetingController TargetingController => targetingController;
        public PlayerWeaponCollector WeaponCollector => weaponCollector;
        public PlayerWeaponController WeaponController => weaponController;

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
                health = GetComponent<Health>();

            if (movement == null)
                movement = GetComponent<PlayerMovement>();

            if (rotation == null)
                rotation = GetComponent<PlayerRotation>();

            if (grabController == null)
                grabController = GetComponent<PlayerGrabController>();

            if (dashController == null)
                dashController = GetComponent<PlayerDashController>();

            if (targetingController == null)
                targetingController = GetComponent<PlayerTargetingController>();

            if (weaponCollector == null)
                weaponCollector = GetComponent<PlayerWeaponCollector>();

            if (weaponController == null)
                weaponController = GetComponent<PlayerWeaponController>();
        }

        public bool IsValid()
        {
            return health != null
                   && movement != null
                   && rotation != null
                   && grabController != null
                   && dashController != null
                   && targetingController != null
                   && weaponCollector != null
                   && weaponController != null;
        }
    }
}