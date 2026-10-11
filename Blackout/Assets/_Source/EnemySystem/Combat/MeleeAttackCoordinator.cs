using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    // Lives on the target. Only arbitrates attack permissions; never moves enemies.
    [DisallowMultipleComponent]
    public sealed class MeleeAttackCoordinator : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxConcurrentAttacks = 2;
        [SerializeField, Min(0.5f)] private float approachTimeout = 2.5f;

        private sealed class Request
        {
            public EnemyMeleeAttack Owner;
            public float Refreshed;
            public float Invited = -1f;
            public float Expires;
            public bool Active;
        }

        private readonly List<Request> requests = new();
        private readonly Stack<Request> recycled = new();

        public int ActiveCount
        {
            get
            {
                Prune();
                int count = 0;
                for (int i = 0; i < requests.Count; i++)
                    if (requests[i].Active) count++;
                return count;
            }
        }

        public bool RequestAttack(EnemyMeleeAttack owner)
        {
            Prune();
            Request request = Find(owner);
            if (request == null)
            {
                request = recycled.Count > 0 ? recycled.Pop() : new Request();
                request.Owner = owner;
                request.Active = false;
                request.Invited = -1f;
                requests.Add(request);
            }
            request.Refreshed = Time.time;
            if (request.Active) return true;

            int available = Mathf.Max(1, maxConcurrentAttacks) - ActiveCount;
            for (int i = 0; i < requests.Count; i++)
            {
                Request candidate = requests[i];
                if (candidate.Active) continue;
                if (--available < 0) return false;
                if (candidate != request) continue;
                if (request.Invited < 0f) request.Invited = Time.time;
                if (Time.time - request.Invited < approachTimeout) return true;

                // An unreachable requester must not monopolize the next permission.
                requests.RemoveAt(i);
                request.Invited = -1f;
                requests.Add(request);
                return false;
            }
            return false;
        }

        public bool TryAcquire(EnemyMeleeAttack owner, float duration)
        {
            if (!isActiveAndEnabled || !RequestAttack(owner)) return false;
            Request request = Find(owner);
            if (request.Active) return true;
            if (ActiveCount >= Mathf.Max(1, maxConcurrentAttacks)) return false;
            request.Active = true;
            request.Expires = Time.time + duration + 1f;
            return true;
        }

        public bool OwnsPermission(EnemyMeleeAttack owner)
        {
            Prune();
            Request request = Find(owner);
            return isActiveAndEnabled && request != null && request.Active;
        }

        public void Release(EnemyMeleeAttack owner)
        {
            for (int i = requests.Count - 1; i >= 0; i--)
                if (requests[i].Owner == owner) Recycle(i);
        }

        private void Recycle(int index)
        {
            Request request = requests[index];
            requests.RemoveAt(index);
            request.Owner = null;
            recycled.Push(request);
        }

        private Request Find(EnemyMeleeAttack owner)
        {
            for (int i = 0; i < requests.Count; i++)
                if (requests[i].Owner == owner) return requests[i];
            return null;
        }

        private void Prune()
        {
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                Request request = requests[i];
                if (request.Owner == null || !request.Owner.IsAvailable ||
                    request.Owner.Target != transform ||
                    (request.Active ? Time.time > request.Expires : Time.time - request.Refreshed > 0.75f))
                    Recycle(i);
            }
        }

        private void OnDisable()
        {
            for (int i = requests.Count - 1; i >= 0; i--) Recycle(i);
        }
        private void OnValidate() => maxConcurrentAttacks = Mathf.Max(1, maxConcurrentAttacks);
    }
}
