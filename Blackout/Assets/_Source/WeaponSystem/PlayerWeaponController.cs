using System.Collections.Generic;
using PlayerSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeaponSystem
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private APlayerWeapon startWeapon;
        [SerializeField] private List<APlayerWeapon> weapons = new();

        private readonly Dictionary<WeaponType, APlayerWeapon> weaponMap = new();

        private PlayerInputReader inputReader;
        private PlayerWeaponCollector weaponCollector;
        private APlayerWeapon currentWeapon;

        public WeaponType CurrentWeaponType => currentWeapon != null
            ? currentWeapon.WeaponType
            : WeaponType.Claws;

        public void Initialize(PlayerInputReader inputReader, PlayerWeaponCollector weaponCollector)
        {
            this.inputReader = inputReader;
            this.weaponCollector = weaponCollector;

            BuildWeaponMap();

            currentWeapon = startWeapon;

            this.inputReader.AttackPressed += Attack;
            this.weaponCollector.WeaponCollected += OnWeaponCollected;

            if (currentWeapon != null)
                Debug.Log($"Start weapon selected: {currentWeapon.WeaponType}");
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                SelectWeapon(WeaponType.Claws);

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                SelectWeapon(WeaponType.Pistol);
        }

        private void OnDestroy()
        {
            if (inputReader != null)
                inputReader.AttackPressed -= Attack;

            if (weaponCollector != null)
                weaponCollector.WeaponCollected -= OnWeaponCollected;
        }

        public void SelectWeapon(WeaponType weaponType)
        {
            if (weaponCollector != null && !weaponCollector.HasWeapon(weaponType))
            {
                Debug.Log($"Weapon is not collected yet: {weaponType}");
                return;
            }

            if (!weaponMap.TryGetValue(weaponType, out APlayerWeapon weapon))
            {
                Debug.LogWarning($"Weapon is not registered: {weaponType}", this);
                return;
            }

            currentWeapon = weapon;

            Debug.Log($"Weapon selected: {weaponType}");
        }

        private void Attack()
        {
            if (currentWeapon == null)
                return;

            currentWeapon.Attack();
        }

        private void OnWeaponCollected(WeaponType weaponType)
        {
            SelectWeapon(weaponType);
        }

        private void BuildWeaponMap()
        {
            weaponMap.Clear();

            foreach (APlayerWeapon weapon in weapons)
            {
                if (weapon == null)
                    continue;

                weaponMap[weapon.WeaponType] = weapon;
            }
        }
    }
}