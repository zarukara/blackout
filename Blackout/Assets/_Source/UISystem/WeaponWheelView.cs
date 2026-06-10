using PlayerSystem;
using UnityEngine;
using WeaponSystem;

namespace UISystem
{
    public class WeaponWheelView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject wheelRoot;

        [Header("Slots")]
        [SerializeField] private WeaponWheelSlotView[] slots;

        [Header("Time")]
        [SerializeField] private float openedTimeScale = 0.15f;

        private PlayerInputReader inputReader;
        private PlayerWeaponCollector weaponCollector;
        private PlayerWeaponController weaponController;

        private WeaponWheelSlotView hoveredSlot;
        private bool isOpened;

        private float previousTimeScale;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLockState;

        public void Initialize(
            PlayerInputReader inputReader,
            PlayerWeaponCollector weaponCollector,
            PlayerWeaponController weaponController
        )
        {
            this.inputReader = inputReader;
            this.weaponCollector = weaponCollector;
            this.weaponController = weaponController;

            this.inputReader.WeaponWheelStarted += Open;
            this.inputReader.WeaponWheelCanceled += CloseAndSelect;

            SubscribeSlots();

            RefreshSlots();

            if (wheelRoot != null)
                wheelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.WeaponWheelStarted -= Open;
                inputReader.WeaponWheelCanceled -= CloseAndSelect;
            }

            UnsubscribeSlots();
        }

        private void Open()
        {
            if (isOpened)
                return;

            if (Time.timeScale <= 0.001f)
                return;

            isOpened = true;
            hoveredSlot = null;

            previousTimeScale = Time.timeScale;
            previousCursorVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;

            // ВАЖНО: сначала обновляем слоты, пока колесо ещё скрыто.
            RefreshSlots();

            if (wheelRoot != null)
                wheelRoot.SetActive(true);

            // Повторно применяем состояние уже после активации,
            // чтобы Button/Overlay точно не показали дефолтный кадр.
            RefreshSlots();
            Canvas.ForceUpdateCanvases();

            Time.timeScale = openedTimeScale;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void CloseAndSelect()
        {
            if (!isOpened)
                return;

            TrySelectHoveredSlot();

            isOpened = false;

            if (wheelRoot != null)
                wheelRoot.SetActive(false);

            Time.timeScale = previousTimeScale;

            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLockState;

            hoveredSlot = null;
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
    }
}