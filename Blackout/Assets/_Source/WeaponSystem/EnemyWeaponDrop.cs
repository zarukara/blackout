using CombatSystem;
using UnityEngine;

namespace WeaponSystem
{
    [RequireComponent(typeof(Health))]
    public class EnemyWeaponDrop : MonoBehaviour
    {
        [Header("Drop")]
        [SerializeField] private WeaponPickup weaponPickupPrefab;
        [SerializeField] private Transform dropPoint;

        [Header("Settings")]
        [SerializeField] private bool dropOnDeath = true;

        private Health health;
        private bool hasDropped;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Died += DropWeapon;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= DropWeapon;
        }

        private void DropWeapon()
        {
            if (!dropOnDeath)
                return;

            if (hasDropped)
                return;

            if (weaponPickupPrefab == null)
            {
                Debug.LogWarning("Weapon pickup prefab is missing.", this);
                return;
            }

            hasDropped = true;

            Vector3 spawnPosition = dropPoint != null
                ? dropPoint.position
                : transform.position;

            Quaternion spawnRotation = Quaternion.identity;

            Instantiate(weaponPickupPrefab, spawnPosition, spawnRotation);
        }
    }
}