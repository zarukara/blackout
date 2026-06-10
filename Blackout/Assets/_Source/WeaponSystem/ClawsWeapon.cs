using PlayerSystem;
using UnityEngine;

namespace WeaponSystem
{
    public class ClawsWeapon : APlayerWeapon
    {
        [Header("References")]
        [SerializeField] private PlayerMeleeAttack playerMeleeAttack;

        public override void Attack()
        {
            if (playerMeleeAttack == null)
                return;

            playerMeleeAttack.PerformAttack();
        }
    }
}