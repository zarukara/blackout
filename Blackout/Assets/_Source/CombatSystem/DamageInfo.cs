using UnityEngine;

namespace CombatSystem
{
    public readonly struct DamageInfo
    {
        public DamageInfo(int amount, bool isLethal, Vector3 point, Vector3 direction, bool hasContact)
        {
            Amount = amount;
            IsLethal = isLethal;
            Point = point;
            Direction = direction;
            HasContact = hasContact;
        }

        public int Amount { get; }
        public bool IsLethal { get; }
        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public bool HasContact { get; }
    }
}
