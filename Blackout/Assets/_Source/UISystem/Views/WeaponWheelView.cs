using PlayerSystem;
using UnityEngine;
using WeaponSystem;

namespace UISystem
{
    public class WeaponWheelView : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private WeaponWheelSlotView[] slots;

        private PlayerInputReader inputReader;
        private PlayerWeaponCollector weaponCollector;
        private PlayerWeaponController weaponController;
        private UiStateController uiStateController;

        private WeaponWheelSlotView hoveredSlot;
        private bool isOpened;

        public void Initialize(
            PlayerInputReader inputReader,
            PlayerWeaponCollector weaponCollector,
            PlayerWeaponController weaponController,
            UiStateController uiStateController
        )
        {
            this.inputReader = inputReader;
            this.weaponCollector = weaponCollector;
            this.weaponController = weaponController;
            this.uiStateController = uiStateController;

            this.inputReader.WeaponWheelStarted += Open;
            this.inputReader.WeaponWheelCanceled += CloseAndSelect;
            this.uiStateController.StateChanged += OnUiStateChanged;

            SubscribeSlots();
            RefreshSlots();
        }

        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.WeaponWheelStarted -= Open;
                inputReader.WeaponWheelCanceled -= CloseAndSelect;
            }

            if (uiStateController != null)
                uiStateController.StateChanged -= OnUiStateChanged;

            UnsubscribeSlots();
        }

        private void Open()
        {
            if (isOpened)
                return;

            hoveredSlot = null;

            RefreshSlots();

            if (!uiStateController.TryOpenWeaponWheel())
                return;

            isOpened = true;

            RefreshSlots();
            Canvas.ForceUpdateCanvases();
        }

        private void CloseAndSelect()
        {
            if (!isOpened)
                return;

            if (!uiStateController.IsState(GameUiState.WeaponWheel))
            {
                isOpened = false;
                hoveredSlot = null;
                return;
            }

            TrySelectHoveredSlot();

            isOpened = false;
            hoveredSlot = null;

            uiStateController.TryCloseWeaponWheel();
        }

        private void TrySelectHoveredSlot()
        {
            if (hoveredSlot == null)
                return;

            if (hoveredSlot.IsLocked)
            {
                Debug.Log($"Weapon is locked: {hoveredSlot.WeaponType}");
                return;
            }

            weaponController.SelectWeapon(hoveredSlot.WeaponType);
        }

        private void RefreshSlots()
        {
            if (slots == null)
                return;

            foreach (WeaponWheelSlotView slot in slots)
            {
                if (slot == null)
                    continue;

                bool isUnlocked = weaponCollector.HasWeapon(slot.WeaponType);
                slot.SetLocked(!isUnlocked);
            }
        }

        private void SubscribeSlots()
        {
            if (slots == null)
                return;

            foreach (WeaponWheelSlotView slot in slots)
            {
                if (slot == null)
                    continue;

                slot.PointerEntered += OnSlotPointerEntered;
            }
        }

        private void UnsubscribeSlots()
        {
            if (slots == null)
                return;

            foreach (WeaponWheelSlotView slot in slots)
            {
                if (slot == null)
                    continue;

                slot.PointerEntered -= OnSlotPointerEntered;
            }
        }

        private void OnSlotPointerEntered(WeaponWheelSlotView slot)
        {
            if (!isOpened)
                return;

            hoveredSlot = slot;
        }

        private void OnUiStateChanged(GameUiState state)
        {
            if (state == GameUiState.WeaponWheel)
                return;

            isOpened = false;
            hoveredSlot = null;
        }
    }
}