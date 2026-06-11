using System;
using UnityEngine;

namespace UISystem
{
    public class UiStateController : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private UiStateScreen[] stateScreens;

        [Header("Time")]
        [SerializeField] private float gameplayTimeScale = 1f;
        [SerializeField] private float weaponWheelTimeScale = 0.15f;
        [SerializeField] private float pausedTimeScale = 0f;

        public GameUiState CurrentState { get; private set; } = GameUiState.Gameplay;

        public event Action<GameUiState> StateChanged;

        public void Initialize()
        {
            CacheScreensIfNeeded();
            SetState(GameUiState.Gameplay);
        }

        [ContextMenu("Cache State Screens")]
        public void CacheScreens()
        {
            stateScreens = GetComponentsInChildren<UiStateScreen>(true);
        }

        public bool TryOpenWeaponWheel()
        {
            if (CurrentState != GameUiState.Gameplay)
                return false;

            SetState(GameUiState.WeaponWheel);
            return true;
        }

        public bool TryCloseWeaponWheel()
        {
            if (CurrentState != GameUiState.WeaponWheel)
                return false;

            SetState(GameUiState.Gameplay);
            return true;
        }

        public void TogglePause()
        {
            if (CurrentState == GameUiState.Death)
                return;

            if (CurrentState == GameUiState.WeaponWheel)
                return;

            if (CurrentState == GameUiState.Pause)
            {
                SetState(GameUiState.Gameplay);
                return;
            }

            if (CurrentState == GameUiState.Gameplay)
                SetState(GameUiState.Pause);
        }

        public void ClosePause()
        {
            if (CurrentState != GameUiState.Pause)
                return;

            SetState(GameUiState.Gameplay);
        }

        public void OpenDeath()
        {
            SetState(GameUiState.Death);
        }

        public bool IsState(GameUiState state)
        {
            return CurrentState == state;
        }

        private void CacheScreensIfNeeded()
        {
            if (stateScreens != null && stateScreens.Length > 0)
                return;

            CacheScreens();
        }

        private void SetState(GameUiState newState)
        {
            if (CurrentState == newState)
            {
                ApplyState();
                return;
            }

            CurrentState = newState;
            ApplyState();

            StateChanged?.Invoke(CurrentState);
        }

        private void ApplyState()
        {
            ApplyScreens();
            Time.timeScale = GetTimeScaleForState(CurrentState);
        }

        private void ApplyScreens()
        {
            if (stateScreens == null)
                return;

            foreach (UiStateScreen screen in stateScreens)
            {
                if (screen == null)
                    continue;

                bool isVisible = screen.VisibleState == CurrentState;
                screen.SetVisible(isVisible);
            }
        }

        private float GetTimeScaleForState(GameUiState state)
        {
            return state switch
            {
                GameUiState.Gameplay => gameplayTimeScale,
                GameUiState.WeaponWheel => weaponWheelTimeScale,
                GameUiState.Pause => pausedTimeScale,
                GameUiState.Death => pausedTimeScale,
                _ => gameplayTimeScale
            };
        }
    }
}