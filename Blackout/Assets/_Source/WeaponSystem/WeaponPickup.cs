using UnityEngine;

namespace WeaponSystem
{
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private WeaponType weaponType = WeaponType.Pistol;

        [Header("Pickup")]
        [SerializeField] private bool destroyAfterPickup = true;

        public WeaponType WeaponType => weaponType;

        private void Awake()
        {
            Collider pickupCollider = GetComponent<Collider>();
            pickupCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerWeaponCollector collector = other.GetComponentInParent<PlayerWeaponCollector>();

            if (collector == null)
                return;

            collector.PickupWeapon(weaponType);

            if (destroyAfterPickup)
                Destroy(gameObject);
        }
    }
}