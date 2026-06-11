using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WeaponSystem;

namespace UISystem
{
    [RequireComponent(typeof(Button))]
    public class WeaponWheelSlotView : MonoBehaviour, IPointerEnterHandler
    {
        [Header("Weapon")]
        [SerializeField] private WeaponType weaponType;

        [Header("State")]
        [SerializeField] private GameObject lockedOverlay;

        private Button button;
        private bool isLocked;

        public WeaponType WeaponType => weaponType;
        public bool IsLocked => isLocked;

        public event Action<WeaponWheelSlotView> PointerEntered;

        private void Awake()
        {
            CacheComponents();
            ApplyState();
        }

        private void OnEnable()
        {
            CacheComponents();
            ApplyState();
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;

            CacheComponents();
            ApplyState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEntered?.Invoke(this);
        }

        private void CacheComponents()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        private void ApplyState()
        {
            if (lockedOverlay != null)
                lockedOverlay.SetActive(isLocked);

            if (button != null)
                button.interactable = !isLocked;
        }
    }
}