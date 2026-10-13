using UnityEngine;

namespace CombatSystem
{
    public static class CombatAim
    {
        public static void AlignMuzzle(Transform weapon, Transform muzzle, Vector3 point, Transform aimPivot = null)
        {
            if (weapon == null || muzzle == null || !muzzle.IsChildOf(weapon)) return;
            // Aim the animated upper body as a unit so both hands retain their authored grip.
            Transform pivot = aimPivot != null && muzzle.IsChildOf(aimPivot) ? aimPivot : weapon;
            // The pivot-to-muzzle offset moves the origin as we turn; converge from that origin.
            for (int i = 0; i < 5; i++)
            {
                Vector3 direction = point - muzzle.position;
                if (direction.sqrMagnitude < 0.0001f) return;
                pivot.rotation = Quaternion.FromToRotation(muzzle.forward, direction) * pivot.rotation;
            }
        }

        public static bool IsFacing(Transform root, Vector3 point, float tolerance)
        {
            Vector3 direction = Vector3.ProjectOnPlane(point - root.position, root.up);
            return direction.sqrMagnitude < 0.0001f || Vector3.Angle(root.forward, direction) <= tolerance;
        }
    }
}
