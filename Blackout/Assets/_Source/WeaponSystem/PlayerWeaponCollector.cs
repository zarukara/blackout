using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    public class PlayerWeaponCollector : MonoBehaviour
    {
        private readonly HashSet<WeaponType> collectedWeapons = new();

        public event Action<WeaponType> WeaponCollected;

        private void Awake()
        {
            collectedWeapons.Add(WeaponType.Claws);
        }

        public bool HasWeapon(WeaponType weaponType)
        {
            return collectedWeapons.Contains(weaponType);
        }

        public void PickupWeapon(WeaponType weaponType)
        {
            if (collectedWeapons.Contains(weaponType))
            {
                Debug.Log($"Weapon already collected: {weaponType}");
                return;
            }

            collectedWeapons.Add(weaponType);

            Debug.Log($"Weapon picked up: {weaponType}");

            WeaponCollected?.Invoke(weaponType);
        }
    }
}