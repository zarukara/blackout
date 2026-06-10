using UnityEngine;

namespace WeaponSystem
{
    public abstract class APlayerWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private WeaponType weaponType;

        public WeaponType WeaponType => weaponType;

        public abstract void Attack();
    }
}